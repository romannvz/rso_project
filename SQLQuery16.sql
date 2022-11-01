select h.name as 'Название штаба', th.name as 'Тип штаба', 
ins.name as 'Название уч.зав.', ins.Location as 'Город' from Headquarters h, Types_Headquarters th, Institutions ins
where h.Id_Type_Headquarter=th.Id_Type_Headquarter and h.Id_Institution=ins.Id_Institution