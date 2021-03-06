﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Character_Builder_Builder
{
    public delegate void SavedEvent(object sender, string id);
    public interface IMainEditor : IHistoryManager
    {
        event SavedEvent Saved;
        bool Save();
        void Exit();
        void ShowPreview();
    }
}
