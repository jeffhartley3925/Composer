namespace Composer.Infrastructure.Constants
{
	public sealed class Defaults
	{
        public const string DefaultImageUrl = "https://www.wecontrib.com/composer/images/person.jpg";

        public const int StaffDimensionWidth = 100; //TODO. Bind to this instead of hardcoded 100 in staffview
        public const int CompositionLeftMargin = 10;
        public const int SequenceIncrement = 100;

        public const int DefaultStaffgroupDensity = 1;
        public const int DefaultSimpleStaffStaffDensity = 1; //must be 1
        public const int DefaultGrandStaffStaffDensity = 2; //must be 2
        public const int DefaultMultiInstrumentStaffDensity = 4;
        public const int DefaultMeasureDensity = 2;

        public const int DefaultTimeSignatureId = 4;

        public const int NewCompositionPanelStaffgroupDensity = 1;
        public const int NewCompositionPanelSimpleStaffConfigurationStaffDensity = 1; //must be 1
        public const int NewCompositionPanelGrandStaffConfigurationStaffDensity = 2; //must be 2
        public const int NewCompositionPanelMeasureDensity = 1;

        public const int MeasureHeight = 145; //should be 130?
        public const int VerseHeight = 14;
        public const int ProvenanceHeight = 111;
        public const int PrintingHeight = 147;

        public const int StaffDimensionClefWidth = 23;
        public const int StaffDimensionTimeSignatureWidth = 10;
        public const int StaffDimensionSpacerWidth = 5;
        public const int staffLinesHeight = 32;
        public const int StaffHeight = 138;

        public const double BracketHeightBaseline = 170;
        public const double BracketScaleYBaseline = 2.6;

        public const int AccidentalFlatWidth = 9;
        public const int AccidentalSharpWidth = 8;
        public const string AccidentalSharpSymbol = "s";
        public const string AccidentalFlatSymbol = "f";
        public const int MinusInfinity = -1000000000;
        public const int PlusInfinity = 1000000000;

        public const char VerseWordPropertyDelimitter = '^';
        public const char VerseWordDelimitter = '~';

        public const double ScrollWidth = 649;
        public const double ScrollHeight = 622;

        public const bool Disable = false;
        public const bool Enable = true;

        public const int AuthorCollaboratorIndex = 0;

	    public const string RestSymbol = "R";

	    public const string AlignCenter = "Center";
        public const string AlignLeft = "Left";
        public const string AlignRight = "Right";
        public const string AlignTop = "Top";
        public const string AlignBottom = "Bottom";

        public const int Activator = 5;
        public const int Deactivator = 7;
    }
}