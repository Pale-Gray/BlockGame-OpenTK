﻿using Blockgame_OpenTK.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Mod
{
    internal interface IMod
    {

        public abstract void OnLoad(Register register);

    }
}
