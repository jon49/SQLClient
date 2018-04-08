# SQL Client

## Idea behind project

The idea behind this project is to be able to generate all the code you need
for a SQL query in your client application. This can you help you get going on
your client side code with minimal effort and makes refactoring easier when, as
a client consumer of your SQL database, you want to just get going.

## Ideas

To get this to work. We would need to be able to send the query to the back
end. We could have the back end accept calls from your front end for code you
are writing. Like so,

```cli
sqlclient --watch --source ./my-directory ./sqlclient.config.json
```

In the config file there could be a setting that defines where to send the
updated query. The query would need to have a unique identifier so the back end
knows which version of the file it would be. So, when you save the file the
following would happen:

![](./images/SQLClient-Flow.svg)

The results from the parsing could be saved in the saved directory which would
contain the following information.

- Namespace
    - Name of the directory in which SQL file resides.
- List of SQL files in namespace.
    - Name of the file.
      - This would also be the name of the method to get the URI for the file.
    - GUID or Hash of file
      - The GUID would be new for every time the file is saved.
      - An alternative would be to hash the file and that would give a unique
        identifier to the results.
      - The code would need to be uploaded each time a save is done to preserve
        versioning not overwriting what is already up on the server.
        Consequently, there might be a lot of code uploaded that would never be
        used. Not sure if there would be a way around this, or if it even
        matters. On the back end you could have some code that periodically
        removes code that is never used. Maybe, if it hasn't been used for a
        month or a year then it is deleted, or it could be kept forever.
    - Results from the parsing
      - sp_describe_undeclared_parameters
      - sys.dm_exec_describe_first_result_set
      - XML Schema
        - Only used if returning a JSON or XML result.
        - For the first iteration neither would be designed in. Then only JSON.
          I don't need XML so that would only be created if someone else did it.
      - ReturnType
        - JSON, XML, or SQL Table (Default)
          - Note that these are the SQL return types. The return types to the
            client could be JSON/XML/CSV/etc. Depending on the rules of your
            back end.

The back end could be just a library that works with these files. So, others
just need to add the library and it will could with just a configuration file.

Since the results from the SQL code are added into a shared file optimizatons
can be made with the resulting code, like inheriting interfaces, like so,

```typescript
interface FirstName {
  FirstName: string
}

interface Actor extends FirstName { }

interface Employee extends FirstName { }
```

How can I accept arrays into the code? Dapper can do it. With
`FSharp.Data.SqlClient` they just require that you have a user defined type in
the database.

### POSTing

When doing an `INSERT` or an `UPDATE` the code could automatically know what
validation is needed and have one method for `validate` and another for
`normalize`. For example,

`validate` could return `firstName: 'Name must be less than x characters
long.'`

And `normalize` would just crop the name automatically to make it valid data.

## Implementation Details

### JSON Return Type

You can't get the types when `FOR JSON` is used on a SQL statement. So, the
code will need to be manipulated to get the results. So, for example, we could
do something like this:

```sql
-- ignore
DECLARE @Id int = 1;
-- start
SELECT TOP 1
-- select
-- root: actor
    t.*
  , film.*
FROM dbo.actor t
OUTER APPLY (
    SELECT f.*
    FROM dbo.film f
    JOIN dbo.film_actor fa
        ON fa.film_id = f.film_id
    WHERE fa.actor_id = t.actor_id
) film
WHERE t.actor_id = @Id;
```

The SQL comment `-- ignore` would be used for when you would like the back end
to fill in the value. In this example we would not want the front end to
actually provide the user ID otherwise they would have access to all users!
Another option would be to use `Row-wise Security` in SQL Server then you
wouldn't even need to declare the variable!

The SQL comment `-- start` would let the parser know that everything before
that point won't be including in the parsing - but those values would be
needed for creating an XML schema if you are generating XML or JSON.

The SQL comment `-- select` tells when the `select` information that you are
actually using starts. The parser would know where this ends by the `FROM` key
keyword. Although we would need to be careful if a query exists in the `SELECT`
statement then you wouldn't want that to mess you up!

The SQL comment `-- root` lets the parser know that you would like a root key
used in the resulting response.

To get the XML schema you can add the following 

```sql
DECLARE @Id int = 1;
-- start
SELECT TOP 1
-- select
-- root: actor
    t.*
  , film.*
FROM dbo.actor t
OUTER APPLY (
    SELECT f.*
    FROM dbo.film f
    JOIN dbo.film_actor fa
        ON fa.film_id = f.film_id
    WHERE fa.actor_id = t.actor_id
) film
WHERE t.actor_id = @Id
FOR XML AUTO, XMLSCHEMA ('actor');
```

The resulting `JSON` query would look something like this:

```sql
SELECT TOP 1
    actor.*
  , JSON_QUERY(REPLACE((SELECT film.* FOR JSON PATH), N'{}', N'')) film
FROM dbo.actor
OUTER APPLY (
    SELECT f.*
    FROM dbo.film f
    JOIN dbo.film_actor fa
        ON fa.film_id = f.film_id
    WHERE fa.actor_id = actor.actor_id
) film
WHERE actor.actor_id = @Id
FOR JSON AUTO, ROOT('actor');
```

If you didn't want the `Film` table to result in multiple results you could add
a `TOP 1` to the query like so:

```sql
...
OUTER APPLY (
    SELECT TOP 1 f.*
    FROM dbo.film f
    JOIN dbo.film_actor fa
        ON fa.film_id = f.film_id
    WHERE fa.actor_id = actor.actor_id
) film
...
```

Which would result in:

```sql  
SELECT TOP 1
    actor.*
  , JSON_QUERY((SELECT film.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)) film
FROM dbo.actor
OUTER APPLY (
    SELECT TOP 1 f.*
    FROM dbo.film f
    JOIN dbo.film_actor fa
        ON fa.film_id = f.film_id
    WHERE fa.actor_id = actor.actor_id
) film
WHERE actor.actor_id = @Id
FOR JSON AUTO, ROOT('actor');
```

### XHR

The method to get the `JSON` from the server could be composable to allow the
caller to be changed. Giving a standard interface so it works automatically
with `fetch` could be the default. And someone could plug in a different
provider if needed.

Since we know the types ahead time items, like `DateTime` could be parsed
automatically when using `JSON.parse`.

