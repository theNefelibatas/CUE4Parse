using System.Text;
using CUE4Parse_Conversion.Formats.Materials;
using CUE4Parse_Conversion.Options;
using CUE4Parse.UE4.Assets.Exports.Material;
using Newtonsoft.Json;

namespace CUE4Parse_Conversion.Exporters;

public sealed class MaterialExporter(UMaterialInterface material) : ExporterBase(material)
{
    protected override IReadOnlyList<ExportFile> BuildExportFiles(CancellationToken ct = default)
    {
        Log.Debug("Extracting material parameters (depth: {Depth})", Session.Options.MaterialDepth);

        var parameters = new CMaterialParams2();
        material.GetParams(parameters, Session.Options.MaterialDepth);

        ExportFile jsonFile;
        if (Session.Options.MaterialJsonFormat == EMaterialJsonFormat.Compact)
        {
            // 紧凑格式（参数视图）：{Textures, Parameters}
            // 属性格式（WriteJson）是默认输出；需要参数视图（供外部工具消费）时选择此项。
            jsonFile = new JsonMaterialFormat().Build(ObjectName, parameters);
        }
        else
        {
            // 属性格式（WriteJson）：{Type, Name, Flags, Class, Package, Properties}
            // 数组包装（[ { ... } ]）与 "Export Folder → Properties" 链路输出一致（该链路输出 exports
            // 数组）；Reflection 的 DeserializeJSON 只接受数组格式，单对象会解析为空而跳过。
            var json = JsonConvert.SerializeObject(new object[] { material }, Formatting.Indented);
            jsonFile = new ExportFile("json", Encoding.UTF8.GetBytes(json));
        }

        var files = new List<ExportFile> { jsonFile };
        if (Session.Options.MeshFormat == EMeshFormat.USD)
        {
            files.Add(new UsdMaterialFormat().Build(ObjectName, parameters, SaveDirectory));
        }

        foreach (var texture in parameters.Textures.Values)
        {
            ct.ThrowIfCancellationRequested();
            Session.Add(texture);
        }

        return files;
    }
}
