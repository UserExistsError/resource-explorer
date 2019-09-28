using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ResourceExplorer
{
    class Win32Resources
    {
        private IntPtr hLibrary;
        private string originalFilename;

        // (HMODULE hMod, WCHAR* type, LONG_PTR params)
        public delegate int EnumResourceTypesDelegate(IntPtr hModule, IntPtr type, IntPtr param);
        public delegate int EnumResourceNamesDelegate(IntPtr hModule, IntPtr type, IntPtr name, IntPtr param);

        // kernel32!LoadLibraryEx flags
        private static UInt32 DONT_RESOLVE_DLL_REFERENCES = 1;
        private static UInt32 LOAD_LIBRARY_AS_DATAFILE = 2;

        private static IDictionary<string, IList<Resource>> instanceResources = new Dictionary<string, IList<Resource>>();
        private static IDictionary<string, Win32Resources> instances = new Dictionary<string, Win32Resources>();

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibraryExW([MarshalAsAttribute(UnmanagedType.LPWStr)] string filepath, IntPtr hFile, UInt32 flags);
        [DllImport("kernel32.dll")]
        public static extern int EnumResourceTypesW(IntPtr hModule, EnumResourceTypesDelegate callback, IntPtr param);
        [DllImport("kernel32.dll")]
        public static extern int EnumResourceNamesW(IntPtr hModule, IntPtr type, EnumResourceNamesDelegate callback, IntPtr param);
        [DllImport("kernel32.dll")]
        public static extern IntPtr FindResourceW(IntPtr hModule, IntPtr name, IntPtr type);
        [DllImport("kernel32.dll")]
        public static extern int SizeofResource(IntPtr hModule, IntPtr hResource);
        [DllImport("kernel32.dll")]
        public static extern int FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResource);
        [DllImport("kernel32.dll")]
        public static extern IntPtr LockResource(IntPtr hGlobal);

        public Win32Resources(string filepath)
        {
            this.originalFilename = filepath;
            // PE arch must match arch of this exe
            this.hLibrary = LoadLibraryExW(filepath, (IntPtr)0, DONT_RESOLVE_DLL_REFERENCES | LOAD_LIBRARY_AS_DATAFILE);
            if (this.hLibrary.ToInt64() == 0)
            {
                throw new Exception("Failed to load file");
            }
            instances[(this.hLibrary.ToString())] = this;
        }

        ~Win32Resources()
        {
            if (this.hLibrary.ToInt64() != 0)
            {
                FreeLibrary(this.hLibrary);
            }
            if (instanceResources.ContainsKey(this.hLibrary.ToString()))
            {
                instanceResources.Remove(this.hLibrary.ToString());
            }
            if (instances.ContainsKey(this.hLibrary.ToString()))
            {
                instances.Remove(this.hLibrary.ToString());
            }
        }

        public IList<Resource> GetResourceList()
        {
            instanceResources[this.hLibrary.ToString()] = new List<Resource>();
            EnumResourceTypesW(this.hLibrary, EnumResourceTypesCallback, (IntPtr)0);
            return instanceResources[this.hLibrary.ToString()];
        }

        public byte[] GetResource(Resource resource, int maxSize = -1)
        {
            return GetResource(resource.type, resource.name, maxSize);
        }

        /*
         * Type and name, if numeric, will be in #NUMBER format
         */
        public byte[] GetResource(string type, string name, int maxSize = -1)
        {
            IList<Resource> resources;
            if (instanceResources.ContainsKey(this.hLibrary.ToString()))
                resources = instanceResources[this.hLibrary.ToString()];
            else
                resources = GetResourceList();
            foreach (Resource r in resources)
            {
                if (r.type == type && r.name == name)
                {
                    IntPtr hGlobal = LoadResource(this.hLibrary, r.hResource);
                    if (hGlobal.ToInt64() == 0)
                    {
                        break;
                    }
                    IntPtr buff = LockResource(hGlobal);
                    if (buff.ToInt64() == 0)
                    {
                        break;
                    }
                    int size = maxSize <= 0 ? r.size : Math.Min(maxSize, r.size);
                    byte[] managedBuff = new byte[size];
                    Marshal.Copy(buff, managedBuff, 0, size);
                    return managedBuff;
                }
            }
            return null; // new byte[0]
        }

        private static int EnumResourceTypesCallback(IntPtr hModule, IntPtr type, IntPtr param)
        {
            EnumResourceNamesW(hModule, type, EnumResourceNamesCallback, param);
            return 1;
        }

        private static Boolean IsIntResource(IntPtr value)
        {
            return (value.ToInt64() >> 16 == 0);
        }

        private static int EnumResourceNamesCallback(IntPtr hModule, IntPtr type, IntPtr name, IntPtr param)
        {
            IntPtr hResource = FindResourceW(hModule, name, type);
            if (hResource.ToInt64() == 0)
            {
                return 0;
            }
            int size = SizeofResource(hModule, hResource);
            if (size == 0)
            {
                return 0;
            }

            string typeString = Marshal.PtrToStringUni(type);
            if (IsIntResource(type))
                typeString = "#" + type.ToString();
            string nameString = Marshal.PtrToStringUni(name);
            if (IsIntResource(name))
                nameString = "#" + name.ToString();

            Win32Resources winr = instances[hModule.ToString()];
            Resource resource = new Resource(winr.originalFilename, hResource, typeString, nameString, size);
            instanceResources[hModule.ToString()].Add(resource);
            resource.setPreview(winr.GetResource(resource, 128));
            return 1;
        }
    }

    class Resource
    {
        public string type;
        public string name;
        public int size;
        public IntPtr hResource;
        private byte[] preview;
        private string filename; // PE file containing the resource

        private static IDictionary<int, string> intTypeMap = new Dictionary<int, string>
        {
            {9, "ACCELERATOR"},
            {21, "ANICURSOR"},
            {22, "ANIICON"},
            {2, "BITMAP"},
            {1, "CURSOR"},
            {5, "DIALOG"},
            {17, "DLGINCLUDE"},
            {8, "FONT"},
            {7, "FONTDIR"},
            {12, "GROUP_CURSOR"},
            {14, "GROUP_ICON"},
            {23, "HTML"},
            {3, "ICON"},
            {24, "MANIFEST"},
            {4, "MENU"},
            {11, "MESSAGETABLE"},
            {19, "PLUGPLAY"},
            {10, "RCDATA"},
            {6, "STRING"},
            {16, "VERSION"},
            {20, "VXD"}
        };

        /*
         * Get a suitable export filename
         */
        public string getDefaultExportName()
        {
            string parentFilename = Path.GetFileNameWithoutExtension(this.filename);
            string nameTmp = name.Replace("#", "0");
            string typeTmp = type.Replace("#", "0");
            string ext = "bin";
            if (getDisplayType() == "STRING")
            {
                ext = "txt";
            }
            return string.Format("{0}_{1}-{2}.{3}", parentFilename, nameTmp, typeTmp, ext);
        }

        public void setPreview(byte[] data)
        {
            this.preview = data;
        }

        public static Encoding GetEncoding(byte[] data)
        {
            if (data.Length >= 3)
            {
                byte[] bom = new byte[3]{
                    data[0], data[1], data[2]
                };
                if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                    return Encoding.UTF8;
                if (bom[0] == 0xff && bom[1] == 0xfe)
                    return Encoding.Unicode;
                if (bom[0] == 0xfe && bom[1] == 0xff)
                    return Encoding.BigEndianUnicode;
                if (bom[1] == 0)
                    return Encoding.Unicode;
                if (bom[0] == 0)
                    return Encoding.BigEndianUnicode;
            }
            return Encoding.Default;
        }

        /*
         * try to determine the encoding
         */
        public Encoding getEncoding()
        {
            return GetEncoding(this.preview);
        }

        public string getDisplayType()
        {
            if (this.type[0] == '#')
            {
                int itype = int.Parse(this.type.Substring(1));
                if (intTypeMap.ContainsKey(itype))
                {
                    return intTypeMap[itype];
                }
            }
            return this.type;
        }

        public string getHexPreview(int size = 32)
        {
            string s = BitConverter.ToString(this.preview).Replace("-", " ");
            return s.Substring(0, Math.Min(s.Length, size));
        }

        public string getContextualPreview(int size = 64)
        {
            // return readable text for string resources
            string s = "";
            if (getDisplayType() == "STRING" || getDisplayType() == "HTML" || getDisplayType() == "MANIFEST")
            {
                s = getEncoding().GetString(this.preview);
            }
            else if (getDisplayType() == "VERSION")
            {
                s = Encoding.Unicode.GetString(this.preview).Substring(3).Replace('\u0000', ' ');
            }
            else
            {
                s = GetString(this.preview);
            }
            return s.Substring(0, Math.Min(s.Length, size));
        }

        public static string GetString(byte[] data)
        {
            Encoding enc = Resource.GetEncoding(data);
            StringBuilder printable = new StringBuilder();
            bool lastnull = false;
            foreach (Char c in enc.GetString(data))
            {
                if (Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c) || Char.IsPunctuation(c))
                {
                    printable.Append(c);
                    lastnull = false;
                }
                else if (c == 0)
                {
                    // replace consecutive nulls with a single space
                    if (!lastnull)
                    {
                        lastnull = true;
                        printable.Append(' ');
                    }
                }
            }
            return printable.ToString();
        }

        public Resource(string filename, IntPtr hResource, string type, string name, int size)
        {
            this.filename = filename;
            this.hResource = hResource;
            this.type = type;
            this.name = name;
            this.size = size;
        }

        public override string ToString()
        {
            return string.Format("type = {0}, name = {1}, size = {2}", this.type, this.name, this.size);
        }

    }
}
