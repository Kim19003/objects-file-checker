// See https://aka.ms/new-console-template for more information
using YamlDotNet.Serialization;

namespace Program
{
    public class Program
    {
        public static int ErrorAmount { get; set; }
        public static int WarningAmount { get; set; }
        public static ConsoleColor DefaultColor { get; } = ConsoleColor.White;

        static void Main()
        {
            IDeserializer deserializer = new DeserializerBuilder()
                .Build();

            PrintHeader();

            string objectsFilePath;
            try
            {
                objectsFilePath = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ObjectsFilePath.txt"))[0];
            }
            catch (Exception ex)
            {
                WriteWhiteText($"Critical failure: {ex.Message}");

                Console.ReadLine();

                return;
            }

            while (true)
            {
                PrintHeader();

                WriteWhiteText($"(1) Print all objects\n(2) Run checker");
                
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D1:
                        PrintAllObjects(deserializer, objectsFilePath);
                        break;
                    case ConsoleKey.D2:
                        RunChecker(deserializer, objectsFilePath);
                        break;
                }

                Console.ReadLine();
            }
        }

        private static void PrintAllObjects(IDeserializer deserializer, string objectsFilePath)
        {
            PrintHeader();

            List<ObjectClass> objectClasses = deserializer.Deserialize<List<ObjectClass>>(File.ReadAllText(objectsFilePath));

            List<Object> allObjects = new();
            foreach (var objectClass in objectClasses)
            {
                allObjects.AddRange(objectClass.Objects);
            }

            WriteWhiteText("(1) Seperate objects with classes\n(2) Sort objects by Id\n(3) Both\n(4) None");

            bool seperateWithClasses = false, sortById = false;

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1:
                    seperateWithClasses = true;
                    break;
                case ConsoleKey.D2:
                    sortById = true;
                    break;
                case ConsoleKey.D3:
                    seperateWithClasses = true;
                    sortById = true;
                    break;
            }

            PrintHeader();

            WriteWhiteText($"Printing all objects started... ({nameof(seperateWithClasses)}: {seperateWithClasses.ToString().ToLower()}, {nameof(sortById)}: {sortById.ToString().ToLower()})\n");

            if (seperateWithClasses)
            {
                foreach (var @class in objectClasses)
                {
                    WriteCyanText($"\n{@class.Class} ({@class.Objects.Count} objects)");
                    WriteGrayText("--------------------");

                    if (sortById)
                    {
                        @class.Objects = @class.Objects.OrderBy(obj => obj.Id).ToList();
                    }

                    foreach (var @object in @class.Objects)
                    {
                        WriteWhiteText($"{@object.Id} = {@object.Name} {{'{@object.Tags}'}}");
                    }
                }

                WriteWhiteText($"\n\nPrinted {objectClasses.Count} classes with total of {allObjects.Count} objects");
            }
            else
            {
                if (sortById)
                {
                    allObjects = allObjects.OrderBy(obj => obj.Id).ToList();
                }

                WriteWhiteText("");
                foreach (var @object in allObjects)
                {
                    WriteWhiteText($"{@object.Id} = {@object.Name} {{'{@object.Tags}'}}");
                }

                WriteWhiteText($"\n\nPrinted {allObjects.Count} objects");
            }
        }

        private static void RunChecker(IDeserializer deserializer, string objectsFilePath)
        {
            PrintHeader();

            WriteWhiteText("File checking started...\n");

            List<ObjectClass> objectClasses = deserializer.Deserialize<List<ObjectClass>>(File.ReadAllText(objectsFilePath));

            List<Object> allObjects = new();
            foreach (var objectClass in objectClasses)
            {
                allObjects.AddRange(objectClass.Objects);
            }

            WriteGrayText($"\nRunning test '{nameof(CheckForUniqueIdsAndNames)}'...");
            CheckForUniqueIdsAndNames(allObjects);

            WriteGrayText($"\nRunning test '{nameof(CheckForIdSequence)}'...");
            CheckForIdSequence(allObjects);

            WriteGrayText($"\nRunning test '{nameof(CheckForCorrectFormatting)}'...");
            CheckForCorrectFormatting(objectClasses, allObjects);

            WriteWhiteText($"\n\nChecking finished with {ErrorAmount} error(s) and {WarningAmount} warning(s) ({objectClasses.Count} classes and {allObjects.Count} objects checked)");
        }

        private static void CheckForUniqueIdsAndNames(List<Object> objects)
        {
            int errors = 0, warnings = 0;

            Dictionary<int, string> ids = new();
            Dictionary<string, int> names = new();

            foreach (var @object in objects)
            {
                if (!ids.ContainsKey(@object.Id))
                {
                    ids.Add(@object.Id, @object.Name);
                }
                else
                {
                    string duplicateObjectName = ids.FirstOrDefault(value => value.Key == @object.Id).Value;
                    WriteErrorText($"Id duplicate '{@object.Id}' found with objects with name '{@object.Name}' and '{duplicateObjectName}'");
                    errors++;
                }

                if (!names.ContainsKey(@object.Name))
                {
                    names.Add(@object.Name, @object.Id);
                }
                else
                {
                    int duplicateObjectId = names.FirstOrDefault(value => value.Key == @object.Name).Value;
                    WriteErrorText($"Name duplicate '{@object.Name}' found with objects with Id '{@object.Id}' and '{duplicateObjectId}'");
                    errors++;
                }
            }

            if (errors < 1 && warnings < 1)
            {
                WriteSuccessfulText("Everything looks good");
            }
        }

        private static void CheckForCorrectFormatting(List<ObjectClass> objectClasses, List<Object> objects)
        {
            int errors = 0, warnings = 0;

            foreach (var objectClass in objectClasses)
            {
                if (objectClass.Class.Length < 1)
                {
                    WriteErrorText($"Nameless class found");
                    errors++;
                }
                else if (!char.IsUpper(objectClass.Class[0]))
                {
                    WriteWarningText($"Class with name '{objectClass.Class}' doesn't start with capital letter");
                    warnings++;
                }
                if (objectClass.Class.Trim() != objectClass.Class)
                {
                    WriteErrorText($"Class with name '{objectClass.Class.Trim()}' contains leading or trailing whitespaces in it's name");
                    errors++;
                }
                else if (objectClass.Class.Contains(' '))
                {
                    WriteWarningText($"Class with name '{objectClass.Class}' contains whitespaces in it's name");
                    warnings++;
                }
            }

            foreach (var @object in objects)
            {
                if (@object.Id < 1)
                {
                    WriteErrorText($"Object with name '{@object.Name}' contains Id below 1");
                    errors++;
                }
                if (@object.Name.Length < 1)
                {
                    WriteErrorText($"Object with Id '{@object.Id}' is nameless");
                    errors++;
                }
                else if (!char.IsUpper(@object.Name.Trim()[0]))
                {
                    WriteWarningText($"Object with name '{@object.Name}' doesn't start with capital letter");
                    warnings++;
                }
                if (@object.Name.Trim() != @object.Name)
                {
                    WriteErrorText($"Object with name '{@object.Name.Trim()}' contains leading or trailing whitespaces in it's name");
                    errors++;
                }
                if (@object.Name.Trim().Contains(' '))
                {
                    try
                    {
                        string[] objectWords = @object.Name.Trim().Split(' ');

                        foreach (var objectWord in objectWords)
                        {
                            if (!char.IsNumber(objectWord[0]) && !char.IsUpper(objectWord[0]))
                            {
                                WriteWarningText($"Object with name '{@object.Name}' has parts in it's name that don't start with capital letter");
                                warnings++;

                                break;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            if (errors < 1 && warnings < 1)
            {
                WriteSuccessfulText("Everything looks good");
            }
        }

        private static void CheckForIdSequence(List<Object> objects)
        {
            int errors = 0, warnings = 0;

            List<int> ids = new();

            foreach (var @object in objects)
            {
                ids.Add(@object.Id);
            }

            ids.Sort();

            int previousId = 0;
            foreach (var id in ids)
            {
                int gap = id - previousId;

                if (gap > 1)
                {
                    WriteErrorText($"There's {gap} number gap between Ids '{previousId}' and '{id}'");
                    errors++;
                }

                previousId = id;
            }

            if (errors < 1 && warnings < 1)
            {
                WriteSuccessfulText("Everything looks good");
            }
        }

        private static void PrintHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("--------------------\nObjects File Checker\n--------------------\n");
            Console.ForegroundColor = DefaultColor;
        }

        private static void WriteSuccessfulText(string text)
        {
            WriteGreenText(text);
        }

        private static void WriteWarningText(string text)
        {
            WriteYellowText(text);

            WarningAmount++;
        }

        private static void WriteErrorText(string text)
        {
            WriteRedText(text);

            ErrorAmount++;
        }

        private static void WriteRedText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = DefaultColor;
        }

        private static void WriteYellowText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ForegroundColor = DefaultColor;
        }

        private static void WriteGreenText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ForegroundColor = DefaultColor;
        }

        private static void WriteWhiteText(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            Console.ForegroundColor = DefaultColor;
        }

        private static void WriteGrayText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(text);
            Console.ForegroundColor = DefaultColor;
        }

        private static void WriteMagentaText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(text);
            Console.ForegroundColor = DefaultColor;
        }

        private static void WriteCyanText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(text);
            Console.ForegroundColor = DefaultColor;
        }
    }

    public class ObjectClass
    {
        public string Class { get; set; }
        public List<Object> Objects { get; set; }
    }

    public class Object
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }
    }

    public class LoadedObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
    }
}