﻿using Blockgame_OpenTK.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Core.Modding;

public interface IModLoader
{

    public abstract void OnLoad(NewRegister register);

}
