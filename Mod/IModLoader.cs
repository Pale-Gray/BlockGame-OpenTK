using Game.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Modding;

public interface IModLoader
{

    public abstract static void OnLoad(Register register);

}
