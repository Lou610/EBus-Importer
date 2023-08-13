select * from module
select * from duty
select * from journey
select * from stage
select * from trans
select * from waybill

delete from module where id_Module >= 1
delete from duty where id_Duty >= 1
delete from journey where id_Journey >= 1
delete from stage where id_Stage >= 1
delete from trans where id_Trans >= 1
delete from waybill where ModuleID >= 1
