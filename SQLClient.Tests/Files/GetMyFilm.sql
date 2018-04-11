SELECT t.film_id, t.[description], t.[length]
FROM dbo.film t
WHERE t.film_id = @Id;
