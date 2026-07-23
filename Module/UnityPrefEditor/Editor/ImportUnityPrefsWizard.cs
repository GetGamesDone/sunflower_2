using UnityEditor;
using UnityEngine;

namespace Sabresaurus.UnityPrefsEditor
{
    public class ImportUnityPrefsWizard : ScriptableWizard
    {
        // Company and product name for importing PlayerPrefs from other projects
        [SerializeField] string importCompanyName = "";
        [SerializeField] string importProductName = "";

        private void OnEnable()
        {
            importCompanyName = PlayerSettings.companyName;
            importProductName = PlayerSettings.productName;
        }

        private void OnInspectorUpdate()
        {
            if (Resources.FindObjectsOfTypeAll(typeof(UnityPrefsEditor)).Length == 0)
            {
                Close();
            }
        }

        protected override bool DrawWizardGUI()
        {
            GUILayout.Label("Import PlayerPrefs from another project, also useful if you change product or company name", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Separator();
            return base.DrawWizardGUI();
        }

        private void OnWizardCreate()
        {
            if (Resources.FindObjectsOfTypeAll(typeof(UnityPrefsEditor)).Length >= 1)
            {
                ((UnityPrefsEditor)Resources.FindObjectsOfTypeAll(typeof(UnityPrefsEditor))[0]).Import(importCompanyName, importProductName);
            }
        }
    }
}
