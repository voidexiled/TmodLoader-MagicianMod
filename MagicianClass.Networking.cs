using System.IO;
using MagicianClass.Content;

namespace MagicianClass;

partial class MagicianClass
{
    internal enum MessageType : byte
    {
        FocusResourceEffect
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        var msgType = (MessageType)reader.ReadByte();
        switch (msgType)
        {
            case MessageType.FocusResourceEffect:
                GlobalPlayer.HandleFocusResourceEffectMessage(reader, whoAmI);
                break;
            default:
                Logger.WarnFormat("MagicianClass: Unknown Message type: {0}", msgType);
                break;
        }
    }
}