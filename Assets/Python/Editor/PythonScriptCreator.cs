using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class PythonScriptCreator
{
    [MenuItem("Assets/Create/Python Script", false, 80)]
    public static void CreateNewPythonScript()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, 
            ScriptableObject.CreateInstance<DoCreatePythonScript>(),
            GetSelectedPathOrFallback() + "/NewPythonScript.py", 
            AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Gizmos/PythonScript Icon.png"),
            string.Empty);
    }

    public static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }
}

class DoCreatePythonScript : EndNameEditAction
{
    static string TemplateText = @"
import PythonBehaviour
import UnityEngine

# Start is called before the first frame update
def Start():
    pass

# Update is called once per frame
def Update():
    pass
";
    
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        UnityEngine.Object o = CreateScriptAssetFromTemplate(pathName, resourceFile);
        ProjectWindowUtil.ShowCreatedAsset(o);
    }

    internal static UnityEngine.Object CreateScriptAssetFromTemplate(string pathName, string resourceFile)
    {
        string fullPath = Path.GetFullPath(pathName);
        string text = TemplateText;
        // string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
        // text = Regex.Replace(text, "#NAME#", fileNameWithoutExtension);
        bool encoderShouldEmitUTF8Identifier = true;
        bool throwOnInvalidBytes = false;
        UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
        bool append = false;
        StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
        streamWriter.Write(text);
        streamWriter.Close();
        AssetDatabase.ImportAsset(pathName);
        return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
    }

}