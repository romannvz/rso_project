select s.name as 'Отряд', d.Name as 'Направление', i.Name as 'Уч.Зав.', h.Name as 'Штаб'
from Squads s, Directions d, Institutions i, Headquarters h
where s.Id_Direction = d.Id_Direction and s.Id_Institution = i.Id_Institution and s.Id_Headquarters = h.Id_Headquarter