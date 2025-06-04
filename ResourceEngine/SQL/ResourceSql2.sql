/* 0. Clean-slate ---------------------------------------------------------- */
DROP TABLE IF EXISTS resource_closure;
DROP TABLE IF EXISTS resource;

/* 1. Base table (still keeps parent_id for convenience) ------------------ */
CREATE TABLE resource
(
    id         SERIAL  PRIMARY KEY,
    name       TEXT    NOT NULL,
    parent_id  INTEGER REFERENCES resource(id) ON DELETE CASCADE
);

/* 2. Closure table: one row per (ancestor, descendant) pair -------------- */
CREATE TABLE resource_closure
(
    ancestor_id   INTEGER NOT NULL REFERENCES resource(id) ON DELETE CASCADE,
    descendant_id INTEGER NOT NULL REFERENCES resource(id) ON DELETE CASCADE,
    depth         INTEGER NOT NULL,               /* 0 = self, 1 = direct child, … */
    PRIMARY KEY (ancestor_id, descendant_id)
);

CREATE INDEX idx_closure_descendant ON resource_closure(descendant_id);

/* 3. Trigger = keep the closure table current on every INSERT ------------ */
CREATE OR REPLACE FUNCTION trg_closure_after_insert()  RETURNS TRIGGER AS
$$
BEGIN
    /* 3a. every node is its own ancestor */
INSERT INTO resource_closure (ancestor_id, descendant_id, depth)
VALUES (NEW.id, NEW.id, 0);

/* 3b. and it inherits all ancestors of its parent (if any) */
IF NEW.parent_id IS NOT NULL THEN
        INSERT INTO resource_closure (ancestor_id, descendant_id, depth)
SELECT
    ancestor_id,
    NEW.id,
    depth + 1
FROM resource_closure
WHERE descendant_id = NEW.parent_id;      -- all ancestors ↑ of the parent
END IF;

RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER resource_closure_ai
    AFTER INSERT ON resource
    FOR EACH ROW EXECUTE FUNCTION trg_closure_after_insert();

/* 4. Test data: 4 levels, 2–3 children each ------------------------------ */

-- level-0 (root)
INSERT INTO resource (name) VALUES ('Root');                  -- id = 1

-- level-1
INSERT INTO resource (name, parent_id) VALUES
                                           ('Section A', 1), ('Section B', 1), ('Section C', 1);       -- ids 2-4

-- level-2
INSERT INTO resource (name, parent_id) VALUES                 -- children of A
                                                              ('Topic A1', 2), ('Topic A2', 2),
                                                              -- children of B
                                                              ('Topic B1', 3), ('Topic B2', 3),
                                                              -- children of C
                                                              ('Topic C1', 4), ('Topic C2', 4);                           -- ids 5-10

-- level-3
INSERT INTO resource (name, parent_id) VALUES                 -- grand-children
                                                              ('Sub A1-α', 5), ('Sub A1-β', 5),
                                                              ('Sub B2-α', 9), ('Sub B2-β', 9);                           -- ids 11-14

/* 5. One-shot query: whole subtree of :root_id --------------------------- */
/* Replace :root_id with a literal or \set root_id 2   in psql */
WITH rel AS (
    SELECT rc.descendant_id,
           rc.depth,
           r.name
    FROM   resource_closure rc
               JOIN   resource r ON r.id = rc.descendant_id
    WHERE  rc.ancestor_id = :root_id
),
     paths AS (
         /* build readable “dotted” paths using string_agg over the closure table */
         SELECT
             d.descendant_id              AS id,
             d.name,
             d.depth,
             string_agg(a.name, ' › ' ORDER BY c.depth) AS path
         FROM       resource_closure c
                        JOIN       resource     a ON a.id = c.ancestor_id
                        JOIN LATERAL (
             SELECT descendant_id, depth, name
             FROM   rel WHERE descendant_id = c.descendant_id   -- restrict to subtree
                 ) d ON true
         WHERE      c.descendant_id IN (SELECT descendant_id FROM rel)      -- subtree
           AND      c.ancestor_id   IN (SELECT descendant_id FROM rel)      --   "
         GROUP BY   d.descendant_id, d.name, d.depth
     )
SELECT id, name, depth, path
FROM   paths
ORDER  BY path;