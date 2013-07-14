
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 05/15/2013 18:55:09
-- Generated from EDMX file: C:\Projects\Composer\Composer.Entities\DataModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [SQL2008R2_848836_cdata];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Notes_Chords]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Notes] DROP CONSTRAINT [FK_Notes_Chords];
GO
IF OBJECT_ID(N'[dbo].[FK_Staffgroups_Compositions]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Staffgroups] DROP CONSTRAINT [FK_Staffgroups_Compositions];
GO
IF OBJECT_ID(N'[dbo].[FK_Measures_Staffs]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Measures] DROP CONSTRAINT [FK_Measures_Staffs];
GO
IF OBJECT_ID(N'[dbo].[FK_Staffs_Staffgroups]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Staffs] DROP CONSTRAINT [FK_Staffs_Staffgroups];
GO
IF OBJECT_ID(N'[dbo].[FK_Sharings_Compositions]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Collaborations] DROP CONSTRAINT [FK_Sharings_Compositions];
GO
IF OBJECT_ID(N'[dbo].[FK_MeasureChord]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Chords] DROP CONSTRAINT [FK_MeasureChord];
GO
IF OBJECT_ID(N'[dbo].[FK_CompositionVerse]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Verses] DROP CONSTRAINT [FK_CompositionVerse];
GO
IF OBJECT_ID(N'[dbo].[FK_CompositionArc]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Arcs] DROP CONSTRAINT [FK_CompositionArc];
GO
IF OBJECT_ID(N'[dbo].[FK_StaffArc]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Arcs] DROP CONSTRAINT [FK_StaffArc];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Chords]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Chords];
GO
IF OBJECT_ID(N'[dbo].[Compositions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Compositions];
GO
IF OBJECT_ID(N'[dbo].[Measures]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Measures];
GO
IF OBJECT_ID(N'[dbo].[Notes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Notes];
GO
IF OBJECT_ID(N'[dbo].[Collaborations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Collaborations];
GO
IF OBJECT_ID(N'[dbo].[Staffgroups]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Staffgroups];
GO
IF OBJECT_ID(N'[dbo].[Staffs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Staffs];
GO
IF OBJECT_ID(N'[dbo].[Verses]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Verses];
GO
IF OBJECT_ID(N'[dbo].[Arcs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Arcs];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Chords'
CREATE TABLE [dbo].[Chords] (
    [Id] uniqueidentifier  NOT NULL,
    [Measure_Id] uniqueidentifier  NOT NULL,
    [Key_Id] smallint  NOT NULL,
    [Location_X] int  NOT NULL,
    [Location_Y] int  NOT NULL,
    [StartTime] float  NULL,
    [Duration] decimal(18,3)  NOT NULL,
    [Audit_Author_Id] nvarchar(max)  NOT NULL,
    [Audit_CreateDate] datetime  NOT NULL,
    [Audit_ModifyDate] datetime  NOT NULL,
    [Audit_CollaboratorIndex] smallint  NULL,
    [Status] nvarchar(max)  NULL
);
GO

-- Creating table 'Compositions'
CREATE TABLE [dbo].[Compositions] (
    [Id] uniqueidentifier  NOT NULL,
    [Instrument_Id] int  NOT NULL,
    [Key_Id] int  NOT NULL,
    [TimeSignature_Id] int  NOT NULL,
    [Status] nvarchar(max)  NULL,
    [Provenance_TitleLine] nvarchar(max)  NOT NULL,
    [Provenance_FontFamily] nvarchar(max)  NOT NULL,
    [Provenance_SmallFontSize] nvarchar(max)  NOT NULL,
    [Provenance_LargeFontSize] nvarchar(max)  NOT NULL,
    [Audit_Author_Id] nvarchar(max)  NOT NULL,
    [Audit_CreateDate] datetime  NOT NULL,
    [Audit_ModifyDate] datetime  NOT NULL,
    [Audit_CollaboratorIndex] smallint  NULL,
    [StaffConfiguration] smallint  NOT NULL,
    [Flags] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Measures'
CREATE TABLE [dbo].[Measures] (
    [Id] uniqueidentifier  NOT NULL,
    [Staff_Id] uniqueidentifier  NOT NULL,
    [TimeSignature_Id] int  NULL,
    [Instrument_Id] int  NOT NULL,
    [Bar_Id] smallint  NOT NULL,
    [Key_Id] smallint  NOT NULL,
    [Width] nvarchar(max)  NOT NULL,
    [Duration] decimal(18,3)  NOT NULL,
    [LedgerColor] nvarchar(max)  NOT NULL,
    [Sequence] int  NOT NULL,
    [Index] smallint  NOT NULL,
    [Audit_Author_Id] nvarchar(max)  NOT NULL,
    [Audit_CreateDate] datetime  NOT NULL,
    [Audit_ModifyDate] datetime  NOT NULL,
    [Audit_CollaboratorIndex] smallint  NULL,
    [Spacing] int  NOT NULL,
    [Status] nvarchar(max)  NULL
);
GO

-- Creating table 'Notes'
CREATE TABLE [dbo].[Notes] (
    [Id] uniqueidentifier  NOT NULL,
    [Chord_Id] uniqueidentifier  NOT NULL,
    [Accidental_Id] int  NULL,
    [Instrument_Id] int  NULL,
    [Key_Id] smallint  NOT NULL,
    [Vector_Id] smallint  NOT NULL,
    [Octave_Id] smallint  NULL,
    [Duration] decimal(18,4)  NOT NULL,
    [Location_X] int  NOT NULL,
    [Location_Y] int  NOT NULL,
    [Audit_Author_Id] nvarchar(max)  NOT NULL,
    [Audit_CreateDate] datetime  NOT NULL,
    [Audit_ModifyDate] datetime  NOT NULL,
    [Audit_CollaboratorIndex] smallint  NULL,
    [Pitch] nvarchar(max)  NULL,
    [Type] smallint  NOT NULL,
    [IsDotted] bit  NULL,
    [StartTime] float  NULL,
    [Orientation] smallint  NULL,
    [IsSpanned] bit  NULL,
    [Status] nvarchar(max)  NULL,
    [Foreground] nvarchar(max)  NOT NULL,
    [Slot] nvarchar(max)  NULL
);
GO

-- Creating table 'Collaborations'
CREATE TABLE [dbo].[Collaborations] (
    [Id] uniqueidentifier  NOT NULL,
    [Composition_Id] uniqueidentifier  NOT NULL,
    [Author_Id] nvarchar(max)  NOT NULL,
    [Collaborator_Id] nvarchar(max)  NOT NULL,
    [Index] int  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [PictureUrl] nvarchar(max)  NULL,
    [Notes] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Staffgroups'
CREATE TABLE [dbo].[Staffgroups] (
    [Id] uniqueidentifier  NOT NULL,
    [Composition_Id] uniqueidentifier  NOT NULL,
    [Sequence] int  NOT NULL,
    [Audit_Author_Id] nvarchar(max)  NOT NULL,
    [Audit_CreateDate] datetime  NOT NULL,
    [Audit_ModifyDate] datetime  NOT NULL,
    [Audit_CollaboratorIndex] smallint  NULL,
    [Key_Id] smallint  NULL,
    [Status] nvarchar(max)  NULL,
    [Index] smallint  NOT NULL
);
GO

-- Creating table 'Staffs'
CREATE TABLE [dbo].[Staffs] (
    [Id] uniqueidentifier  NOT NULL,
    [Clef_Id] int  NULL,
    [Bar_Id] smallint  NOT NULL,
    [Key_Id] smallint  NULL,
    [TimeSignature_Id] int  NOT NULL,
    [Staffgroup_Id] uniqueidentifier  NOT NULL,
    [Sequence] int  NOT NULL,
    [Audit_Author_Id] nvarchar(max)  NOT NULL,
    [Audit_CreateDate] datetime  NOT NULL,
    [Audit_ModifyDate] datetime  NOT NULL,
    [Audit_CollaboratorIndex] smallint  NULL,
    [Status] nvarchar(max)  NULL,
    [Index] smallint  NOT NULL
);
GO

-- Creating table 'Verses'
CREATE TABLE [dbo].[Verses] (
    [Id] uniqueidentifier  NOT NULL,
    [Composition_Id] uniqueidentifier  NOT NULL,
    [Index] smallint  NOT NULL,
    [Text] nvarchar(max)  NOT NULL,
    [Sequence] int  NOT NULL,
    [Audit_Author_Id] nvarchar(max)  NOT NULL,
    [Audit_CreateDate] datetime  NOT NULL,
    [Audit_ModifyDate] datetime  NOT NULL,
    [Audit_CollaboratorIndex] smallint  NULL,
    [Status] nvarchar(max)  NULL,
    [Disposition] smallint  NULL,
    [UIHelper] nvarchar(max)  NULL
);
GO

-- Creating table 'Arcs'
CREATE TABLE [dbo].[Arcs] (
    [Id] uniqueidentifier  NOT NULL,
    [Audit_Author_Id] nvarchar(max)  NOT NULL,
    [Audit_CreateDate] datetime  NOT NULL,
    [Audit_ModifyDate] datetime  NOT NULL,
    [Audit_CollaboratorIndex] smallint  NULL,
    [Composition_Id] uniqueidentifier  NOT NULL,
    [Note_Id1] uniqueidentifier  NOT NULL,
    [Note_Id2] uniqueidentifier  NOT NULL,
    [Chord_Id1] uniqueidentifier  NOT NULL,
    [Chord_Id2] uniqueidentifier  NOT NULL,
    [Type] smallint  NOT NULL,
    [Status] nvarchar(max)  NULL,
    [ArcSweep] nvarchar(max)  NOT NULL,
    [FlareSweep] nvarchar(max)  NOT NULL,
    [Angle] float  NULL,
    [X1] smallint  NULL,
    [Y1] smallint  NULL,
    [X2] smallint  NULL,
    [Y2] smallint  NULL,
    [Top] float  NOT NULL,
    [Left] float  NULL,
    [Staff_Id] uniqueidentifier  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Chords'
ALTER TABLE [dbo].[Chords]
ADD CONSTRAINT [PK_Chords]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Compositions'
ALTER TABLE [dbo].[Compositions]
ADD CONSTRAINT [PK_Compositions]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Measures'
ALTER TABLE [dbo].[Measures]
ADD CONSTRAINT [PK_Measures]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Notes'
ALTER TABLE [dbo].[Notes]
ADD CONSTRAINT [PK_Notes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Collaborations'
ALTER TABLE [dbo].[Collaborations]
ADD CONSTRAINT [PK_Collaborations]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Staffgroups'
ALTER TABLE [dbo].[Staffgroups]
ADD CONSTRAINT [PK_Staffgroups]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Staffs'
ALTER TABLE [dbo].[Staffs]
ADD CONSTRAINT [PK_Staffs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Verses'
ALTER TABLE [dbo].[Verses]
ADD CONSTRAINT [PK_Verses]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Arcs'
ALTER TABLE [dbo].[Arcs]
ADD CONSTRAINT [PK_Arcs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Chord_Id] in table 'Notes'
ALTER TABLE [dbo].[Notes]
ADD CONSTRAINT [FK_Notes_Chords]
    FOREIGN KEY ([Chord_Id])
    REFERENCES [dbo].[Chords]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Notes_Chords'
CREATE INDEX [IX_FK_Notes_Chords]
ON [dbo].[Notes]
    ([Chord_Id]);
GO

-- Creating foreign key on [Composition_Id] in table 'Staffgroups'
ALTER TABLE [dbo].[Staffgroups]
ADD CONSTRAINT [FK_Staffgroups_Compositions]
    FOREIGN KEY ([Composition_Id])
    REFERENCES [dbo].[Compositions]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Staffgroups_Compositions'
CREATE INDEX [IX_FK_Staffgroups_Compositions]
ON [dbo].[Staffgroups]
    ([Composition_Id]);
GO

-- Creating foreign key on [Staff_Id] in table 'Measures'
ALTER TABLE [dbo].[Measures]
ADD CONSTRAINT [FK_Measures_Staffs]
    FOREIGN KEY ([Staff_Id])
    REFERENCES [dbo].[Staffs]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Measures_Staffs'
CREATE INDEX [IX_FK_Measures_Staffs]
ON [dbo].[Measures]
    ([Staff_Id]);
GO

-- Creating foreign key on [Staffgroup_Id] in table 'Staffs'
ALTER TABLE [dbo].[Staffs]
ADD CONSTRAINT [FK_Staffs_Staffgroups]
    FOREIGN KEY ([Staffgroup_Id])
    REFERENCES [dbo].[Staffgroups]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Staffs_Staffgroups'
CREATE INDEX [IX_FK_Staffs_Staffgroups]
ON [dbo].[Staffs]
    ([Staffgroup_Id]);
GO

-- Creating foreign key on [Composition_Id] in table 'Collaborations'
ALTER TABLE [dbo].[Collaborations]
ADD CONSTRAINT [FK_Sharings_Compositions]
    FOREIGN KEY ([Composition_Id])
    REFERENCES [dbo].[Compositions]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Sharings_Compositions'
CREATE INDEX [IX_FK_Sharings_Compositions]
ON [dbo].[Collaborations]
    ([Composition_Id]);
GO

-- Creating foreign key on [Measure_Id] in table 'Chords'
ALTER TABLE [dbo].[Chords]
ADD CONSTRAINT [FK_MeasureChord]
    FOREIGN KEY ([Measure_Id])
    REFERENCES [dbo].[Measures]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_MeasureChord'
CREATE INDEX [IX_FK_MeasureChord]
ON [dbo].[Chords]
    ([Measure_Id]);
GO

-- Creating foreign key on [Composition_Id] in table 'Verses'
ALTER TABLE [dbo].[Verses]
ADD CONSTRAINT [FK_CompositionVerse]
    FOREIGN KEY ([Composition_Id])
    REFERENCES [dbo].[Compositions]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CompositionVerse'
CREATE INDEX [IX_FK_CompositionVerse]
ON [dbo].[Verses]
    ([Composition_Id]);
GO

-- Creating foreign key on [Composition_Id] in table 'Arcs'
ALTER TABLE [dbo].[Arcs]
ADD CONSTRAINT [FK_CompositionArc]
    FOREIGN KEY ([Composition_Id])
    REFERENCES [dbo].[Compositions]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CompositionArc'
CREATE INDEX [IX_FK_CompositionArc]
ON [dbo].[Arcs]
    ([Composition_Id]);
GO

-- Creating foreign key on [Staff_Id] in table 'Arcs'
ALTER TABLE [dbo].[Arcs]
ADD CONSTRAINT [FK_StaffArc]
    FOREIGN KEY ([Staff_Id])
    REFERENCES [dbo].[Staffs]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_StaffArc'
CREATE INDEX [IX_FK_StaffArc]
ON [dbo].[Arcs]
    ([Staff_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------