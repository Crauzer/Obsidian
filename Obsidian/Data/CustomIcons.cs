﻿namespace Obsidian.Data;

public static class CustomIcons
{
    public static class Material
    {
        public static readonly string Archive = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24""><title>archive</title><path d=""M3,3H21V7H3V3M4,8H20V21H4V8M9.5,11A0.5,0.5 0 0,0 9,11.5V13H15V11.5A0.5,0.5 0 0,0 14.5,11H9.5Z"" /></svg>";
        public static readonly string ArchiveEdit = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24""><title>archive-edit</title><path d=""M20 10.3V8H4V21H11V19.13L19.39 10.74C19.57 10.56 19.78 10.42 20 10.3M15 13H9V11.5C9 11.22 9.22 11 9.5 11H14.5C14.78 11 15 11.22 15 11.5V13M21 7H3V3H21V7M22.85 14.19L21.87 15.17L19.83 13.13L20.81 12.15C21 11.95 21.33 11.95 21.53 12.15L22.85 13.47C23.05 13.67 23.05 14 22.85 14.19M19.13 13.83L21.17 15.87L15.04 22H13V19.96L19.13 13.83Z"" /></svg>";
        public static readonly string CodeJson = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24""><title>code-json</title><path d=""M5,3H7V5H5V10A2,2 0 0,1 3,12A2,2 0 0,1 5,14V19H7V21H5C3.93,20.73 3,20.1 3,19V15A2,2 0 0,0 1,13H0V11H1A2,2 0 0,0 3,9V5A2,2 0 0,1 5,3M19,3A2,2 0 0,1 21,5V9A2,2 0 0,0 23,11H24V13H23A2,2 0 0,0 21,15V19A2,2 0 0,1 19,21H17V19H19V14A2,2 0 0,1 21,12A2,2 0 0,1 19,10V5H17V3H19M12,15A1,1 0 0,1 13,16A1,1 0 0,1 12,17A1,1 0 0,1 11,16A1,1 0 0,1 12,15M8,15A1,1 0 0,1 9,16A1,1 0 0,1 8,17A1,1 0 0,1 7,16A1,1 0 0,1 8,15M16,15A1,1 0 0,1 17,16A1,1 0 0,1 16,17A1,1 0 0,1 15,16A1,1 0 0,1 16,15Z"" /></svg>";
        public static readonly string ContentSave = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24""><title>content-save</title><path d=""M15,9H5V5H15M12,19A3,3 0 0,1 9,16A3,3 0 0,1 12,13A3,3 0 0,1 15,16A3,3 0 0,1 12,19M17,3H5C3.89,3 3,3.9 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V7L17,3Z"" /></svg>";
        public static readonly string ContentSaveAll = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24""><title>content-save-all</title><path d=""M17,7V3H7V7H17M14,17A3,3 0 0,0 17,14A3,3 0 0,0 14,11A3,3 0 0,0 11,14A3,3 0 0,0 14,17M19,1L23,5V17A2,2 0 0,1 21,19H7C5.89,19 5,18.1 5,17V3A2,2 0 0,1 7,1H19M1,7H3V21H17V23H3A2,2 0 0,1 1,21V7Z"" /></svg>";
        public static readonly string FilterCog = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24""><title>filter-cog</title><path d=""M22.77 19.32L21.7 18.5C21.72 18.33 21.74 18.17 21.74 18S21.73 17.67 21.7 17.5L22.76 16.68C22.85 16.6 22.88 16.47 22.82 16.36L21.82 14.63C21.76 14.5 21.63 14.5 21.5 14.5L20.27 15C20 14.82 19.73 14.65 19.42 14.53L19.23 13.21C19.22 13.09 19.11 13 19 13H17C16.87 13 16.76 13.09 16.74 13.21L16.55 14.53C16.25 14.66 15.96 14.82 15.7 15L14.46 14.5C14.35 14.5 14.22 14.5 14.15 14.63L13.15 16.36C13.09 16.47 13.11 16.6 13.21 16.68L14.27 17.5C14.25 17.67 14.24 17.83 14.24 18S14.25 18.33 14.27 18.5L13.21 19.32C13.12 19.4 13.09 19.53 13.15 19.64L14.15 21.37C14.21 21.5 14.34 21.5 14.46 21.5L15.7 21C15.96 21.18 16.24 21.35 16.55 21.47L16.74 22.79C16.76 22.91 16.86 23 17 23H19C19.11 23 19.22 22.91 19.24 22.79L19.43 21.47C19.73 21.34 20 21.18 20.27 21L21.5 21.5C21.63 21.5 21.76 21.5 21.83 21.37L22.83 19.64C22.89 19.53 22.86 19.4 22.77 19.32M18 19.5C17.16 19.5 16.5 18.83 16.5 18S17.17 16.5 18 16.5 19.5 17.17 19.5 18 18.83 19.5 18 19.5M3 3C2.78 3 2.57 3.08 2.38 3.22C1.95 3.56 1.87 4.19 2.21 4.62L7.97 12H8V17.87C7.96 18.16 8.06 18.47 8.29 18.7L10.3 20.71C10.65 21.06 11.19 21.08 11.58 20.8C11.2 19.91 11 18.96 11 18C11 16.73 11.35 15.5 12 14.4V12H12.03L17.79 4.62C18.13 4.19 18.05 3.56 17.62 3.22C17.43 3.08 17.22 3 17 3H3Z"" /></svg>";
        public static readonly string Subscript = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24""><title>format-subscript</title><path d=""M16,7.41L11.41,12L16,16.59L14.59,18L10,13.41L5.41,18L4,16.59L8.59,12L4,7.41L5.41,6L10,10.59L14.59,6L16,7.41M21.85,21.03H16.97V20.03L17.86,19.23C18.62,18.58 19.18,18.04 19.56,17.6C19.93,17.16 20.12,16.75 20.13,16.36C20.14,16.08 20.05,15.85 19.86,15.66C19.68,15.5 19.39,15.38 19,15.38C18.69,15.38 18.42,15.44 18.16,15.56L17.5,15.94L17.05,14.77C17.32,14.56 17.64,14.38 18.03,14.24C18.42,14.1 18.85,14 19.32,14C20.1,14.04 20.7,14.25 21.1,14.66C21.5,15.07 21.72,15.59 21.72,16.23C21.71,16.79 21.53,17.31 21.18,17.78C20.84,18.25 20.42,18.7 19.91,19.14L19.27,19.66V19.68H21.85V21.03Z"" /></svg>";
    }
}
