public static class WhoOGTranscript {

    public static readonly string[] displays = new string[13] { "DISPLAY", "PRESS", "PRESSED", "LAST", "START", "ONE", "STRIKES", "TO", "SCREEN", "TWO", "RESET", "DISARMED", "STRIKE" };
    public static readonly string[][] words = new string[13][]
    {
        new string[] { "READ", "THE", "DISPLAY", "AND", "FIND", "THE", "LINE", "IN", "THIS", "MANUAL", "WHERE", "ITS", "WORD", "IS", "WRITTEN", "IN", "BOLD"},
        new string[] { "PRESS", "THE", "BUTTONS", "WHOSE", "LABELS", "MATCH", "THE", "WORDS", "IN", "THAT", "LINE", "IN", "THE", "ORDER", "THEY", "ARE", "GIVEN"},
        new string[] { "A", "BUTTON", "MAY", "ONLY", "BE", "PUSHED", "ONCE", "UNLESS", "ANOTHER", "BUTTON", "MUST", "BE", "PRESSED", "IN", "BETWEEN"},
        new string[] { "NOTE", "THE", "POSITION", "OF", "THE", "BUTTON", "THAT", "WAS", "PRESSED", "LAST", "BEFORE", "THE", "MODULE", "BEGINS", "THE", "SECOND", "PHASE", "OF", "ITS", "STAGE"},
        new string[] { "IN", "THE", "SECOND", "PHASE", "EACH", "BUTTON", "WILL", "HAVE", "A", "SINGLE", "SYMBOL", "AS", "ITS", "LABEL", "AND", "A", "SIXTY", "SECOND", "TIMER", "WILL", "START", "COUNTING", "DOWN", "TO", "ZERO"},
        new string[] { "ONLY", "ONE", "BUTTON", "WILL", "DISPLAY", "A", "VALID", "SYMBOL", "USE", "THE", "RULES", "BELOW", "TO", "FIND", "WHICH", "OF", "THEM", "IT", "IS"},
        new string[] { "IN", "THE", "GRID", "GIVEN", "BY", "THE", "NUMBER", "OF", "STRIKES", "USE", "THE", "ROW", "OR", "COLUMN", "MATCHING", "THE", "POSITION", "OF", "THE", "LAST", "PRESSED", "BUTTON"},
        new string[] { "IF", "THE", "WORD", "DETONATE", "IS", "WRITTEN", "ON", "THE", "DISPLAY", "THE", "VALID", "SYMBOL", "DOES", "NOT", "BELONG", "TO", "THAT", "ROW", "OR", "COLUMN"},
        new string[] { "HOWEVER", "IF", "VENT", "GAS", "IS", "ON", "THE", "SCREEN", "INSTEAD", "THE", "SYMBOL", "IN", "THAT", "COLUMN", "OR", "ROW", "IS", "VALID"},
        new string[] { "DO", "NOT", "PRESS", "A", "BUTTON", "WHOSE", "SYMBOL", "IS", "IN", "THE", "SAME", "POSITION", "OF", "ITS", "CELL", "OF", "THE", "GRID", "AS", "A", "PREVIOUS", "VALID", "SYMBOL", "FROM", "THIS", "STAGE", "EVEN", "IF", "IT", "MEETS", "THE", "TWO", "RULES", "ABOVE"},
        new string[] { "WHEN", "THE", "CORRECT", "BUTTON", "IS", "PRESSED", "THE", "TIMER", "IS", "RESET", "AND", "A", "NEW", "SYMBOL", "WILL", "BE", "VALID", "REPEAT", "THE", "STEPS", "ABOVE", "TWO", "MORE", "TIMES", "TO", "COMPLETE", "ONE", "STAGE", "OF", "THE", "MODULE"},
        new string[] { "REPEAT", "EACH", "OF", "THE", "STEPS", "IN", "THIS", "MANUAL", "AGAIN", "UNTIL", "EVERY", "OTHER", "STAGE", "IS", "COMPLETE", "AND", "THE", "MODULE", "IS", "DISARMED"},
        new string[] { "PUSHING", "THE", "WRONG", "BUTTON", "AT", "ANY", "POINT", "OR", "RUNNING", "OUT", "OF", "TIME", "WILL", "RESET", "THE", "CURRENT", "STAGE", "AND", "GIVE", "A", "STRIKE"}
    };
    public static readonly string[][] grids = new string[3][]
    {
        new string[36] { "\u0531", "\u0d09", "\u0b95", "\u0c84", "\u0ca0", "\u0532", "\u0d0b", "\u0b85", "\u0b9a", "\u0d12", "\u0ca5", "\u053d", "\u0d17", "\u0ca8", "\u0533", "\u0b9c", "\u0cac", "\u0ba8", "\u0d24", "\u0536", "\u0539", "\u0cb2", "\u0bb4", "\u0d28", "\u0beb", "\u0d30", "\u0541", "\u0c8c", "\u0d34", "\u0c90", "\u0b89", "\u0554", "\u0556", "\u0b8e", "\u0d5c", "\u0c9f"},
        new string[36] { "\u0ba8", "\u0554", "\u0cb2", "\u0b8e", "\u0d24", "\u0b9c", "\u0d30", "\u0bb4", "\u0536", "\u0d34", "\u0cac", "\u0c90", "\u0d28", "\u0b89", "\u053d", "\u0d0b", "\u0b95", "\u0556", "\u0c9f", "\u0ca5", "\u0beb", "\u0c8c", "\u0d12", "\u0d17", "\u0532", "\u0b9a", "\u0ca0", "\u0d5c", "\u0539", "\u0c84", "\u0d09", "\u0533", "\u0b85", "\u0531", "\u0541", "\u0ca8"},
        new string[36] { "\u0c9f", "\u0536", "\u0d34", "\u0539", "\u0d5c", "\u0d28", "\u0c8c", "\u0beb", "\u0b89", "\u0533", "\u0ca0", "\u0532", "\u0c90", "\u0b85", "\u0556", "\u0d12", "\u0b8e", "\u0531", "\u0b9a", "\u0541", "\u0d30", "\u0554", "\u0c84", "\u0d09", "\u0bb4", "\u0cac", "\u0ca5", "\u0d24", "\u053d", "\u0d17", "\u0ca8", "\u0cb2", "\u0d0b", "\u0b95", "\u0b9c", "\u0ba8"}
    };
}
