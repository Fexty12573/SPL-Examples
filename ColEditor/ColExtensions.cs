
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Components;
using SharpPluginLoader.Core.IO;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Models;
using SharpPluginLoader.Core.MtTypes;
using SharpPluginLoader.Core.Resources;
using SharpPluginLoader.Core.Resources.Collision;

namespace ColEditor;

public static class ColExtensions
{
    private const uint IceborneMagic = 0x18091001;
    private static readonly byte[] ColMagic = [0x43, 0x4F, 0x4C, 0x00];
    private const uint ColVersion = 0x6F;
    private const uint ColIndexVersion = ColVersion + 5;

    public static void SerializeTo(this ObjCollision col, MtStream stream)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // ObjCollision Header
        bw.Write(IceborneMagic);
        bw.Write(ColMagic);
        bw.Write(ColVersion);
        bw.Write(0);

        // CollIndex
        bw.Write(ColIndexVersion);

        if (col.CollIndex is not null)
        {
            var clid = col.CollIndex;
            bw.Write(clid.Indices.Length);

            foreach (ref var index in clid.Indices)
            {
                unsafe { bw.Write(new ReadOnlySpan<byte>(index.NamePtr->Data, index.NamePtr->Length)); }
                bw.Write((byte)0); // null terminator
                bw.Write(index.NodeIndex);
                bw.Write(index.AttackParamIndex);
                bw.Write(index.AppendParamIndex);
                bw.Write(index.LinkId);
                bw.Write(index.LinkTop);
                bw.Write(index.UniqueId);
            }
        }
        else
        {
            bw.Write(0);
        }

        stream.Write(ms.ToArray());

        col.CollNode?.SaveTo(stream);
        col.AttackParam?.SaveTo(stream);
        col.ObjAppendParam?.SaveTo(stream);

        stream.Write([
            ..BitConverter.GetBytes(col.Get<uint>(0xD0)),
            ..BitConverter.GetBytes(col.Get<uint>(0xD8))
        ]);
    }

    public static nint GetCollIndex(this CollisionComponent.Node node)
    {
        return node.Get<nint>(0x98);
    }

    public static unsafe ref CollIndex GetCollIndexObj(this CollisionComponent.Node node)
    {
        return ref MemoryUtil.AsRef(node.GetPtr<CollIndex>(0x98));
    }

    public static void RequestKill(this CollisionComponent.Node node)
    {
        node.Set(0xB0, true);
    }

    public static MtArray<MtObject> GetGeometryArray(this CollGeomResource clgm)
    {
        return clgm.GetInlineObject<MtArray<MtObject>>(0xA8);
    }

    public static unsafe Span<ModelJoint> GetJoints(this Model model)
    {
        return new Span<ModelJoint>(model.GetPtr<ModelJoint>(0x4A8), model.Get<int>(0x4A0));
    }
}
