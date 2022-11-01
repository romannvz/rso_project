CREATE TABLE [dbo].[Headquarters] (
    [Id_Headquarter]       INT           IDENTITY (1, 1) NOT NULL,
    [Id_Type_Headquarter]         INT NOT NULL,
    [Id_Head]            INT NOT NULL,
    [Id_Vice]        INT NOT NULL,
    [Id_Institution]    INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id_Headquarter] ASC)
);

