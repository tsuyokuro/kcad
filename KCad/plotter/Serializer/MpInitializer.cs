using MessagePack;

namespace Plotter.Serializer
{
    [MessagePackObject]
    public class MpDummy
    {
        [Key("value")]
        int value = 0;
    }

    public class MpInitializer
    {
        public static void Init()
        {
            MpDummy v = new MpDummy();

            byte[] b = MessagePackSerializer.Serialize(v);

            v = MessagePackSerializer.Deserialize<MpDummy>(b);
        }
    }
}