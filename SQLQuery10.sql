select m.surname,m.name,m.patronymic,m.birthday,m.phone 
from members m, squads s 
where s.id_headquarters = (select id_headquarters from headquarters where name = 'МосОблРСО') 
and m.id_squad != (select id_squad from squads where name = 'СтудАтом') 
and m.Id_Squad = s.Id_Squad 
order by m.surname,m.name,m.patronymic