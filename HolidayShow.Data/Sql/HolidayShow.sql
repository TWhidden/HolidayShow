﻿-- Holiday Show SQL database creator script
DECLARE @instructionName nvarchar(50);

-- Version table is needed 
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Versions]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Versions](
	[VersionNumber] [int] NOT NULL,
	[DateUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_Versions] PRIMARY KEY CLUSTERED 
(
	[VersionNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END


-- Initial creation

if NOT EXISTS (select * from Versions where VersionNumber = 1)
BEGIN


CREATE TABLE [dbo].[AudioOptions](
	[AudioId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[FileName] [nvarchar](255) NOT NULL,
	[AudioDuration] [int] NOT NULL CONSTRAINT [DF_AudioOptions_AudioDuration]  DEFAULT ((0)),
	[IsNotVisable] [bit] NOT NULL CONSTRAINT [DF_AudioOptions_IsNotVisable]  DEFAULT ((0)),
 CONSTRAINT [PK_AudioOptions] PRIMARY KEY CLUSTERED 
(
	[AudioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[DeviceIoPorts](
	[DeviceIoPortId] [int] IDENTITY(1,1) NOT NULL,
	[DeviceId] [int] NOT NULL,
	[CommandPin] [int] NOT NULL,
	[Description] [nvarchar](50) NOT NULL CONSTRAINT [DF_DeviceIoPorts_Description]  DEFAULT (''),
	[IsNotVisable] [bit] NOT NULL CONSTRAINT [DF_DeviceIoPorts_IsNotVisable]  DEFAULT ((0)),
	[IsDanger] [bit] NOT NULL CONSTRAINT [DF_DeviceIoPorts_IsDanger]  DEFAULT ((0)),
 CONSTRAINT [PK_DeviceIoPorts] PRIMARY KEY CLUSTERED 
(
	[DeviceIoPortId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[DevicePatterns](
	[DevicePatternId] [int] IDENTITY(1,1) NOT NULL,
	[DeviceId] [int] NOT NULL,
	[PatternName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_DevicePatterns] PRIMARY KEY CLUSTERED 
(
	[DevicePatternId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]



CREATE TABLE [dbo].[DevicePatternSequences](
	[DevicePatternSeqenceId] [int] IDENTITY(1,1) NOT NULL,
	[DevicePatternId] [int] NOT NULL,
	[OnAt] [int] NOT NULL,
	[Duration] [int] NOT NULL,
	[AudioId] [int] NOT NULL,
	[DeviceIoPortId] [int] NOT NULL,
 CONSTRAINT [PK_DevicePatternSequences] PRIMARY KEY CLUSTERED 
(
	[DevicePatternSeqenceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Devices](
	[DeviceId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL CONSTRAINT [DF_Devices_Name]  DEFAULT ('NONAME'),
 CONSTRAINT [PK_Devices] PRIMARY KEY CLUSTERED 
(
	[DeviceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Sets](
	[SetId] [int] IDENTITY(1,1) NOT NULL,
	[SetName] [nvarchar](50) NOT NULL,
	[IsDisabled] [bit] NOT NULL CONSTRAINT [DF_Sets_IsDisabled]  DEFAULT ((0)),
 CONSTRAINT [PK_Sets] PRIMARY KEY CLUSTERED 
(
	[SetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[SetSequences](
	[SetSequenceId] [int] IDENTITY(1,1) NOT NULL,
	[SetId] [int] NOT NULL,
	[OnAt] [int] NOT NULL,
	[DevicePatternId] [int] NOT NULL,
 CONSTRAINT [PK_SetSequences] PRIMARY KEY CLUSTERED 
(
	[SetSequenceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Settings](
	[SettingName] [varchar](50) NOT NULL,
	[ValueString] [varchar](max) NOT NULL,
	[ValueDouble] [float] NOT NULL,
 CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED 
(
	[SettingName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[DeviceIoPorts]  WITH CHECK ADD  CONSTRAINT [FK_DeviceIoPorts_Devices] FOREIGN KEY([DeviceId])
REFERENCES [dbo].[Devices] ([DeviceId])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[DeviceIoPorts] CHECK CONSTRAINT [FK_DeviceIoPorts_Devices]

ALTER TABLE [dbo].[DevicePatterns]  WITH CHECK ADD  CONSTRAINT [FK_DevicePatterns_Devices] FOREIGN KEY([DeviceId])
REFERENCES [dbo].[Devices] ([DeviceId])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[DevicePatterns] CHECK CONSTRAINT [FK_DevicePatterns_Devices]

ALTER TABLE [dbo].[DevicePatternSequences]  WITH CHECK ADD  CONSTRAINT [FK_DevicePatternSequences_AudioOptions1] FOREIGN KEY([AudioId])
REFERENCES [dbo].[AudioOptions] ([AudioId])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[DevicePatternSequences] CHECK CONSTRAINT [FK_DevicePatternSequences_AudioOptions1]

ALTER TABLE [dbo].[DevicePatternSequences]  WITH CHECK ADD  CONSTRAINT [FK_DevicePatternSequences_DeviceIoPorts] FOREIGN KEY([DeviceIoPortId])
REFERENCES [dbo].[DeviceIoPorts] ([DeviceIoPortId])

ALTER TABLE [dbo].[DevicePatternSequences] CHECK CONSTRAINT [FK_DevicePatternSequences_DeviceIoPorts]

ALTER TABLE [dbo].[DevicePatternSequences]  WITH CHECK ADD  CONSTRAINT [FK_DevicePatternSequences_DevicePatterns] FOREIGN KEY([DevicePatternId])
REFERENCES [dbo].[DevicePatterns] ([DevicePatternId])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[DevicePatternSequences] CHECK CONSTRAINT [FK_DevicePatternSequences_DevicePatterns]

ALTER TABLE [dbo].[SetSequences]  WITH CHECK ADD  CONSTRAINT [FK_SetSequences_DevicePatterns] FOREIGN KEY([DevicePatternId])
REFERENCES [dbo].[DevicePatterns] ([DevicePatternId])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[SetSequences] CHECK CONSTRAINT [FK_SetSequences_DevicePatterns]

ALTER TABLE [dbo].[SetSequences]  WITH CHECK ADD  CONSTRAINT [FK_SetSequences_Sets] FOREIGN KEY([SetId])
REFERENCES [dbo].[Sets] ([SetId])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[SetSequences] CHECK CONSTRAINT [FK_SetSequences_Sets]

INSERT INTO AudioOptions (Name, [FileName], AudioDuration, IsNotVisable) 
			Values ('NONE', '', 0, 1)

INSERT into VERSIONS (VersionNumber, DateUpdated) Values (1, getUtcDate())


END


if NOT EXISTS (select * from Versions where VersionNumber = 2)
BEGIN
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EffectInstructionsAvailable]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [dbo].[EffectInstructionsAvailable](
		[EffectInstructionId] [int] IDENTITY(1,1) NOT NULL,
		[DisplayName] [nvarchar](50) NOT NULL,
		[InstructionName] [nvarchar](50) NOT NULL,
		[InstructionsForUse] [nvarchar](2000) NOT NULL,
		[IsDisabled] [bit] NOT NULL,
	 CONSTRAINT [PK_EffectInstructionsAvailable] PRIMARY KEY CLUSTERED 
	(
		[EffectInstructionId] ASC
	) ON [PRIMARY]
	) ON [PRIMARY]
	END
	

	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The Friendly name of the instruction' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EffectInstructionsAvailable', @level2type=N'COLUMN',@level2name=N'DisplayName'

	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The internal instruction name' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EffectInstructionsAvailable', @level2type=N'COLUMN',@level2name=N'InstructionName'

	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeviceEffects]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [dbo].[DeviceEffects](
		[EffectId] [int] IDENTITY(1,1) NOT NULL,
		[EffectName] [nvarchar](50) NOT NULL,
		[InstructionMetaData] [nvarchar](500) NOT NULL,
		[Duration] [int] NOT NULL,
		[EffectInstructionId] [int] NOT NULL,
	 CONSTRAINT [PK_DeviceEffects] PRIMARY KEY CLUSTERED 
	(
		[EffectId] ASC
	) ON [PRIMARY]
	) ON [PRIMARY]
	END

	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DeviceEffects_EffectInstructionsAvailable]') AND parent_object_id = OBJECT_ID(N'[dbo].[DeviceEffects]'))
	ALTER TABLE [dbo].[DeviceEffects]  WITH CHECK ADD  CONSTRAINT [FK_DeviceEffects_EffectInstructionsAvailable] FOREIGN KEY([EffectInstructionId])
	REFERENCES [dbo].[EffectInstructionsAvailable] ([EffectInstructionId])

	IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DeviceEffects_EffectInstructionsAvailable]') AND parent_object_id = OBJECT_ID(N'[dbo].[DeviceEffects]'))
	ALTER TABLE [dbo].[DeviceEffects] CHECK CONSTRAINT [FK_DeviceEffects_EffectInstructionsAvailable]

	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The display name of the instruction' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'DeviceEffects', @level2type=N'COLUMN',@level2name=N'EffectName'

	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'metadata that gets sent into the instruction for execution' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'DeviceEffects', @level2type=N'COLUMN',@level2name=N'InstructionMetaData'

	ALTER TABLE dbo.SetSequences ADD EffectId int NULL
	
	ALTER TABLE [dbo].[SetSequences]  WITH CHECK ADD  CONSTRAINT [FK_SetSequences_DeviceEffects] FOREIGN KEY([EffectId])
	REFERENCES [dbo].[DeviceEffects] ([EffectId])
	ON UPDATE CASCADE
	ON DELETE SET NULL
	

	ALTER TABLE [dbo].[SetSequences] CHECK CONSTRAINT [FK_SetSequences_DeviceEffects]
	


INSERT into VERSIONS (VersionNumber, DateUpdated) Values (2, getUtcDate())
END




if NOT EXISTS (select * from Versions where VersionNumber = 3)
BEGIN

	ALTER TABLE dbo.SetSequences
		DROP CONSTRAINT FK_SetSequences_DeviceEffects

	ALTER TABLE dbo.DeviceEffects SET (LOCK_ESCALATION = TABLE)

	ALTER TABLE dbo.SetSequences
		DROP CONSTRAINT FK_SetSequences_Sets
	ALTER TABLE dbo.Sets SET (LOCK_ESCALATION = TABLE)
	ALTER TABLE dbo.SetSequences
		DROP CONSTRAINT FK_SetSequences_DevicePatterns
	ALTER TABLE dbo.DevicePatterns SET (LOCK_ESCALATION = TABLE)
	CREATE TABLE dbo.Tmp_SetSequences
		(
		SetSequenceId int NOT NULL IDENTITY (1, 1),
		SetId int NOT NULL,
		OnAt int NOT NULL,
		DevicePatternId int NULL,
		EffectId int NULL
		)  ON [PRIMARY]
	ALTER TABLE dbo.Tmp_SetSequences SET (LOCK_ESCALATION = TABLE)
	SET IDENTITY_INSERT dbo.Tmp_SetSequences ON
	IF EXISTS(SELECT * FROM dbo.SetSequences)
		 EXEC('INSERT INTO dbo.Tmp_SetSequences (SetSequenceId, SetId, OnAt, DevicePatternId, EffectId)
			SELECT SetSequenceId, SetId, OnAt, DevicePatternId, EffectId FROM dbo.SetSequences WITH (HOLDLOCK TABLOCKX)')
	SET IDENTITY_INSERT dbo.Tmp_SetSequences OFF
	DROP TABLE dbo.SetSequences
	EXECUTE sp_rename N'dbo.Tmp_SetSequences', N'SetSequences', 'OBJECT' 
	ALTER TABLE dbo.SetSequences ADD CONSTRAINT
		PK_SetSequences PRIMARY KEY CLUSTERED 
		(
		SetSequenceId
		) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	ALTER TABLE dbo.SetSequences ADD CONSTRAINT
		FK_SetSequences_DevicePatterns FOREIGN KEY
		(
		DevicePatternId
		) REFERENCES dbo.DevicePatterns
		(
		DevicePatternId
		) ON UPDATE  CASCADE 
		 ON DELETE  CASCADE 
	
	ALTER TABLE dbo.SetSequences ADD CONSTRAINT
		FK_SetSequences_Sets FOREIGN KEY
		(
		SetId
		) REFERENCES dbo.Sets
		(
		SetId
		) ON UPDATE  CASCADE 
		 ON DELETE  CASCADE 
	
	ALTER TABLE dbo.SetSequences ADD CONSTRAINT
		FK_SetSequences_DeviceEffects FOREIGN KEY
		(
		EffectId
		) REFERENCES dbo.DeviceEffects
		(
		EffectId
		) ON UPDATE  CASCADE 
		 ON DELETE  SET NULL 

	ALTER TABLE dbo.SetSequences DROP CONSTRAINT FK_SetSequences_DevicePatterns
	ALTER TABLE dbo.DevicePatterns SET (LOCK_ESCALATION = TABLE)
	ALTER TABLE dbo.SetSequences ADD CONSTRAINT
		FK_SetSequences_DevicePatterns FOREIGN KEY
		(
		DevicePatternId
		) REFERENCES dbo.DevicePatterns
		(
		DevicePatternId
		) ON UPDATE  CASCADE 
		 ON DELETE  SET NULL 
	ALTER TABLE dbo.SetSequences SET (LOCK_ESCALATION = TABLE)
	

	INSERT into VERSIONS (VersionNumber, DateUpdated) Values (3, getUtcDate())
END


if NOT EXISTS (select * from Versions where VersionNumber = 4)
BEGIN

	SET @instructionName = 'GPIO_RANDOM'
	IF NOT EXISTS (select * from [EffectInstructionsAvailable] where [InstructionName] = @instructionName)
	BEGIN
		INSERT INTO [EffectInstructionsAvailable] ([DisplayName], [InstructionName], [InstructionsForUse], [IsDisabled])
												VALUES('GPIO Random', @instructionName, 'Set the GPIO ports that participate in this. MetaData to look like DEVPINS=4:1,4:2;DUR=75;  DUR is the amount of time the GPIO is ON', 0)
	END


	SET @instructionName = 'GPIO_STROBE'
	IF NOT EXISTS (select * from [EffectInstructionsAvailable] where [InstructionName] = @instructionName)
	BEGIN
		INSERT INTO [EffectInstructionsAvailable] ([DisplayName], [InstructionName], [InstructionsForUse], [IsDisabled])
												VALUES('GPIO Strobe', @instructionName, 'Strobes at a regular rate for the length of the current set or the effect duration. Set the effect duration to 0 for set duration. DEVPINS=4:1,4:2;DUR=75;  DUR is the lenght of time the GPIO will be ON', 0)
	END

	SET @instructionName = 'GPIO_STAY_ON'
	IF NOT EXISTS (select * from [EffectInstructionsAvailable] where [InstructionName] = @instructionName)
	BEGIN
		INSERT INTO [EffectInstructionsAvailable] ([DisplayName], [InstructionName], [InstructionsForUse], [IsDisabled])
												VALUES('GPIO ON', @instructionName, 'On until duration is done, or set is over. DEVPINS=4:1,4:2,4:3,4:4,4:5,4:6,4:7,4:8;', 0)
	END
	

	INSERT into VERSIONS (VersionNumber, DateUpdated) Values (4, getUtcDate())
END

-- Feature #8 - Strobe Effect over period of time
if NOT EXISTS (select * from Versions where VersionNumber = 5)
BEGIN

	SET @instructionName = 'GPIO_STROBE_DELAY'
	IF NOT EXISTS (select * from [EffectInstructionsAvailable] where [InstructionName] = @instructionName)
	BEGIN
		INSERT INTO [EffectInstructionsAvailable] ([DisplayName], [InstructionName], [InstructionsForUse], [IsDisabled])
												VALUES('GPIO Strobe Delay', @instructionName, 'Strobes the lights, but has a programmed delay after a period of time. MetaData to look like DEVPINS=4:1,4:2;DUR=75;DELAYBETWEEN=10000;EXECUTEFOR=3000;  DUR is the amount of time the GPIO is ON, DELAYBETWEEN is the amount of time everything is off before restarting, EXECUTEFOR is the amount of time the strobe will run for', 0)
	END

	INSERT into VERSIONS (VersionNumber, DateUpdated) Values (5, getUtcDate())
END

-- Feature #8 - Strobe Effect over period of time
if NOT EXISTS (select * from Versions where VersionNumber = 6)
BEGIN

	SET @instructionName = 'GPIO_SEQUENTIAL'
	IF NOT EXISTS (select * from [EffectInstructionsAvailable] where [InstructionName] = @instructionName)
	BEGIN
		INSERT INTO [EffectInstructionsAvailable] ([DisplayName], [InstructionName], [InstructionsForUse], [IsDisabled])
												VALUES('GPIO Sequential', @instructionName, 'Sequential processing of the GPIO pins, with the programmed delay, and optional "Reverase" for the next pass to go the other direction; DEVPINS=4:1,4:2;DUR=75;REVERSE=1;', 0)
	END

	INSERT into VERSIONS (VersionNumber, DateUpdated) Values (6, getUtcDate())
END

if NOT EXISTS (select * from Versions where VersionNumber = 7)
BEGIN

	ALTER TABLE dbo.DeviceEffects ADD
		TimeOn nvarchar(8) NOT NULL CONSTRAINT DF_DeviceEffects_TimeOn DEFAULT '',
		TimeOff nvarchar(8) NOT NULL CONSTRAINT DF_DeviceEffects_TimeOff DEFAULT ''

	INSERT into VERSIONS (VersionNumber, DateUpdated) Values (7, getUtcDate())
END
