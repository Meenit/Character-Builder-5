﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Character_Builder_5
{
    public class PlayerClass
    {
        public List<int> ClassLevelAtLevel;
        public List<int> HProllAtClassLevel;
        [XmlElement(ElementName = "Class")]
        public String ClassName { get; set; }
        [XmlElement(ElementName = "SubClass")]
        public String SubClassName { get; set; }
        public PlayerClass()
        {
            ClassName = null;
            SubClassName = null;
            ClassLevelAtLevel = new List<int>();
            HProllAtClassLevel = new List<int>();
        }
        public PlayerClass(ClassDefinition classdefinition, int atlevel, int hproll)
        {
            ClassName = classdefinition.Name;
            SubClassName = null;
            ClassLevelAtLevel = new List<int>() {atlevel};
            HProllAtClassLevel = new List<int>() {hproll};
        }
        [XmlIgnore]
        public ClassDefinition Class
        {
            get
            {
                if (ClassName == null || ClassName == "") return null;
                return ClassDefinition.Get(ClassName, null);
            }
            set
            {
                if (value == null) ClassName = "";
                else ClassName = value.Name + " " + ConfigManager.SourceSeperator + " " + value.Source;
            }
        }
        [XmlIgnore]
        public SubClass SubClass
        {
            get
            {
                if (SubClassName == null || SubClassName == "") return null;
                string source = null;
                if (Class != null) source = Class.Source;
                return SubClass.Get(SubClassName, source);
            }
            set
            {
                if (value == null) SubClassName = "";
                else SubClassName = value.Name + " " + ConfigManager.SourceSeperator + " " + value.Source;
            }
        }
        public List<Feature> getFeatures(int level, Player player)
        {
            if (Class == null) return new List<Feature>();
            int classlevel = getClassLevelUpToLevel(level);
            bool secondclass = !ClassLevelAtLevel.Contains(1);
            List<Feature> fl = Class.CollectFeatures(classlevel, secondclass, player);
            if (SubClass != null) fl.AddRange(SubClass.CollectFeatures(classlevel, secondclass, player));
            return fl;
        }
        public int getClassLevelUpToLevel(int level)
        {
            if (level < ClassLevelAtLevel[0]) return 0;
            for (int c = 0; c < ClassLevelAtLevel.Count; c++) if (level<=ClassLevelAtLevel[c]) return c+1;
            return ClassLevelAtLevel.Count;
        }
        public int getHP(int classlevel)
        {
            int hp = 0;
            for (int c = 0; c < classlevel; c++) hp += HProllAtClassLevel[c];
            return hp;
        }
        public bool AddLevel(int atlevel, int hproll)
        {
            ClassLevelAtLevel.Add(atlevel);
            ClassLevelAtLevel.Sort();
            HProllAtClassLevel.Insert(ClassLevelAtLevel.IndexOf(atlevel), hproll);
            return true;
        }
        public int getClassLevelAtLevel(int atlevel)
        {
            return ClassLevelAtLevel.IndexOf(atlevel) + 1;
        }
        public int HPRollAtLevel(int atlevel)
        {
            if (ClassLevelAtLevel.IndexOf(atlevel)<0) return 0;
            return HProllAtClassLevel[ClassLevelAtLevel.IndexOf(atlevel)];
        }
        public int HPAtLevel(int level)
        {
            int classlevel = getClassLevelUpToLevel(level);
            int hp = 0;
            for (int c = 0; c < classlevel; c++) hp += HProllAtClassLevel[c];
            return hp;
        }
        public void setHPRollAtLevel(int atlevel, int hproll)
        {
            if (ClassLevelAtLevel.IndexOf(atlevel) < 0) return;
            HProllAtClassLevel[ClassLevelAtLevel.IndexOf(atlevel)] = hproll;
        }
        public bool deleteLevel(int atlevel)
        {
            if (ClassLevelAtLevel.Count < 2) return false;
            int classlevel = ClassLevelAtLevel.IndexOf(atlevel);
            if (classlevel < 0) return false;
            HProllAtClassLevel.RemoveAt(classlevel);
            ClassLevelAtLevel.RemoveAt(classlevel);
            return true;
        }
        public override string ToString()
        {
            Character_Builder_5.SubClass s = SubClass;
            if (s != null && s.SheetName != null && s.SheetName.Length > 0) return s.SheetName + " " + getClassLevelUpToLevel(Player.current.getLevel());
            if (SubClassName != null && SubClassName != "") return SubClassName + " " + getClassLevelUpToLevel(Player.current.getLevel());
            return ClassName + " " + getClassLevelUpToLevel(Player.current.getLevel());
        }

        public int getMulticlassingLevel(int level=0)
        {
            if (level == 0) level = Player.current.getLevel();
            int classlevel = getClassLevelUpToLevel(level);
            if (classlevel == 0) return 0;
            SubClass sc = SubClass;
            if (sc!=null && sc.MulticlassingSpellLevels != null && sc.MulticlassingSpellLevels.Count >= classlevel) return sc.MulticlassingSpellLevels[classlevel - 1];
            ClassDefinition cl=Class;
            if (cl == null) return 0;
            if (cl.MulticlassingSpellLevels != null && cl.MulticlassingSpellLevels.Count >= classlevel) return cl.MulticlassingSpellLevels[classlevel - 1];
            return 0;
        }

        public void AddControls(List<System.Windows.Forms.Control> control, int level, Player player)
        {
            ControlAdder.AddControls(Class, control, level);
            int classlevel=getClassLevelUpToLevel(level);
            foreach (Feature f in getFeatures(level, player).OrderBy(a=>a.Level)) ControlAdder.AddControl(control, classlevel, f);
        }
    }
}
