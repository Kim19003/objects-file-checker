// See https://aka.ms/new-console-template for more information
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace Program
{
    public class Program
    {
        public static int ErrorAmount { get; set; }
        public static int WarningAmount { get; set; }

        static void Main()
        {
            string objectsFilePath = @"D:\stuff\- STUFF -\object test.yaml";

            IDeserializer deserializer = new DeserializerBuilder()
                .Build();

            WriteWhiteInformationText("Objects file checking started...\n");

            List<ObjectClass> objectClasses = deserializer.Deserialize<List<ObjectClass>>(File.ReadAllText(objectsFilePath));

            List<Object> allObjects = new();
            foreach (var objectClass in objectClasses)
            {
                allObjects.AddRange(objectClass.Objects);
            }

            WriteGrayInformationtext($"\nRunning test '{nameof(CheckForUniqueIdsAndNames)}'...");
            CheckForUniqueIdsAndNames(allObjects);
            WriteGrayInformationtext($"...finished");

            WriteGrayInformationtext($"\nRunning test '{nameof(CheckForIdSequence)}'...");
            CheckForIdSequence(allObjects);
            WriteGrayInformationtext($"...finished");

            WriteGrayInformationtext($"\nRunning test '{nameof(CheckForCorrectFormatting)}'...");
            CheckForCorrectFormatting(objectClasses, allObjects);
            WriteGrayInformationtext($"...finished");

            WriteWhiteInformationText($"\n\nChecking finished with {ErrorAmount} error(s) and {WarningAmount} warning(s) ({objectClasses.Count} classes and {allObjects.Count} objects checked)");

            Console.ReadLine();
        }

        private static void CheckForUniqueIdsAndNames(List<Object> objects)
        {
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
                    WriteErrorText($"- Id duplicate '{@object.Id}' found with objects with name '{@object.Name}' and '{duplicateObjectName}'");
                }

                if (!names.ContainsKey(@object.Name))
                {
                    names.Add(@object.Name, @object.Id);
                }
                else
                {
                    int duplicateObjectId = names.FirstOrDefault(value => value.Key == @object.Name).Value;
                    WriteErrorText($"- Name duplicate '{@object.Name}' found with objects with Id '{@object.Id}' and '{duplicateObjectId}'");
                }
            }
        }

        private static void CheckForCorrectFormatting(List<ObjectClass> objectClasses, List<Object> objects)
        {
            foreach (var objectClass in objectClasses)
            {
                if (objectClass.Class.Length < 1)
                {
                    WriteErrorText($"- Nameless class found");
                }
                else if (!char.IsUpper(objectClass.Class[0]))
                {
                    WriteWarningText($"- Class with name '{objectClass.Class}' doesn't start with capital letter");
                }
                if (objectClass.Class.Trim() != objectClass.Class)
                {
                    WriteErrorText($"- Class with name '{objectClass.Class.Trim()}' contains leading or trailing whitespaces in it's name");
                }
                else if (objectClass.Class.Contains(' '))
                {
                    WriteWarningText($"- Class with name '{objectClass.Class}' contains whitespaces in it's name");
                }
            }

            foreach (var @object in objects)
            {
                if (@object.Id < 1)
                {
                    WriteErrorText($"- Object with name '{@object.Name}' contains Id below 1");
                }
                if (@object.Name.Length < 1)
                {
                    WriteErrorText($"- Object with Id '{@object.Id}' is nameless");
                }
                else if (!char.IsUpper(@object.Name.Trim()[0]))
                {
                    WriteWarningText($"- Object with name '{@object.Name}' doesn't start with capital letter");
                }
                if (@object.Name.Trim() != @object.Name)
                {
                    WriteErrorText($"- Object with name '{@object.Name.Trim()}' contains leading or trailing whitespaces in it's name");
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
                                WriteWarningText($"- Object with name '{@object.Name}' has parts in it's name that don't start with capital letter");

                                break;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static void CheckForIdSequence(List<Object> objects)
        {
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
                    WriteErrorText($"- There's {gap} number gap between Ids '{previousId}' and '{id}'");
                }

                previousId = id;
            }
        }

        private static void WriteWhiteInformationText(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void WriteGrayInformationtext(string text)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void WriteSuccessfulText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void WriteWarningText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;

            WarningAmount++;
        }

        private static void WriteErrorText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;

            ErrorAmount++;
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