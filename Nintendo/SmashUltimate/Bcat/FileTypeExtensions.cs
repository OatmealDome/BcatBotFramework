using System;

namespace Nintendo.SmashUltimate.Bcat
{
    public static class FileTypeExtensions
    {
        public static bool IsContainer(this FileType fileType)
        {
            return (fileType == FileType.Event || fileType == FileType.LineNews || fileType == FileType.PopUpNews || fileType == FileType.Present);
        }

        public static string GetNamePrefixFromType(this FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Event:
                    return "event";
                case FileType.LineNews:
                    return "line_news";
                case FileType.PopUpNews:
                    return "popup_news";
                case FileType.Present:
                    return "present";
                default:
                    throw new Exception("No standard prefix exists for Common files");
            }
        }

        public static FileType GetTypeFromName(string name)
        {
            if (name.StartsWith("event"))
            {
                return FileType.Event;
            }
            else if (name.StartsWith("line"))
            {
                return FileType.LineNews;
            }
            else if (name.StartsWith("popup"))
            {
                return FileType.PopUpNews;
            }
            else if (name.StartsWith("present"))
            {
                return FileType.Present;
            }
            else
            {
                // Assume common
                return FileType.Common;
            }
        }

        public static FileType GetTypeFromContainer(Container container)
        {
            if (container is Event)
            {
                return FileType.Event;
            }
            else if (container is LineNews)
            {
                return FileType.LineNews;
            }
            else if (container is PopUpNews)
            {
                return FileType.PopUpNews;
            }
            else if (container is Present)
            {
                return FileType.Present;
            }
            else
            {
                throw new Exception("Invalid container type");
            }
        }

        public static FileType[] GetAllFileTypes()
        {
            return (FileType[])Enum.GetValues(typeof(FileType));
        }

    }
}