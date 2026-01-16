using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 获取当前选中的 LevelData 对象
        LevelData data = (LevelData)target;

        // 设置全局 Label 宽度 (防止 Label 占用太多空间，给输入框留更多位置)
        EditorGUIUtility.labelWidth = 120;

        // ============================================================
        // 1. 基础配置 (Task Database & Level Number)
        // ============================================================
        EditorGUILayout.LabelField("Base Configuration", EditorStyles.boldLabel);
        data.taskDatabase = (TaskDatabase)EditorGUILayout.ObjectField("Task Database", data.taskDatabase, typeof(TaskDatabase), false);
        data.levelNumber = EditorGUILayout.IntField("Level Number", data.levelNumber);

        // ============================================================
        // 2. 顾客列表绘制
        // ============================================================
        if (data.customers == null) 
            data.customers = new List<LevelCustomer>();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Total Customers: {data.customers.Count}", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        for (int i = 0; i < data.customers.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            LevelCustomer c = data.customers[i];

            // --------------------------------------------------------
            // A. 顾客基本信息
            // --------------------------------------------------------
            EditorGUILayout.LabelField($"Customer {i + 1} Info", EditorStyles.boldLabel);
            
            // ⭐ 改大：Customer Name
            EditorGUILayout.LabelField("Customer Name:");
            c.customerName = EditorGUILayout.TextField(c.customerName, GUILayout.Height(25)); 

            c.customerSprite = (Sprite)EditorGUILayout.ObjectField("Default Sprite", c.customerSprite, typeof(Sprite), false);
            
            // ⭐ 改大：Opening Message
            EditorGUILayout.LabelField("Opening Message:");
            EditorStyles.textField.wordWrap = true; // 允许自动换行
            c.message = EditorGUILayout.TextArea(c.message, GUILayout.Height(50)); // 增加高度

            // --------------------------------------------------------
            // B. 结算表情图片
            // --------------------------------------------------------
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Result Images (Optional)", EditorStyles.miniBoldLabel);
            c.successCustomerSprite = (Sprite)EditorGUILayout.ObjectField("Success Sprite", c.successCustomerSprite, typeof(Sprite), false);
            c.failureCustomerSprite = (Sprite)EditorGUILayout.ObjectField("Failure Sprite", c.failureCustomerSprite, typeof(Sprite), false);

            // --------------------------------------------------------
            // C. 结算对白
            // --------------------------------------------------------
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settlement Messages", EditorStyles.miniBoldLabel);
            
            // ⭐ 改大：Success Message
            EditorGUILayout.LabelField("Success Msg:");
            c.finalSuccessMessage = EditorGUILayout.TextArea(c.finalSuccessMessage, GUILayout.Height(40));

            // ⭐ 改大：Failure Message
            EditorGUILayout.LabelField("Fail Msg:");
            c.finalFailMessage = EditorGUILayout.TextArea(c.finalFailMessage, GUILayout.Height(40));

            // --------------------------------------------------------
            // D. 步骤列表 (Steps)
            // --------------------------------------------------------
            if (c.steps == null) 
                c.steps = new List<LevelStep>();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Steps (Total: {c.steps.Count})", EditorStyles.boldLabel);

            for (int s = 0; s < c.steps.Count; s++)
            {
                EditorGUILayout.BeginVertical("helpbox");
                LevelStep step = c.steps[s];

                EditorGUILayout.LabelField($"Step {s + 1}", EditorStyles.miniBoldLabel);

                // --- 任务类型选择 (Task Type) ---
                TaskDatabase db = data.taskDatabase;
                string[] taskOptions = { "kill", "check", "protect", "encrypt", "decrypt" };
                
                int taskIndex = Array.IndexOf(taskOptions, step.taskType);
                if (taskIndex < 0) taskIndex = 0; 
                
                int newTaskIndex = EditorGUILayout.Popup("Task Type", taskIndex, taskOptions);
                step.taskType = taskOptions[newTaskIndex];

                // --- 子类型选择 (Sub Type) ---
                if (db != null)
                {
                    string[] subOptions = GetSubOptions(db, step.taskType);
                    
                    if (subOptions.Length > 0)
                    {
                        int subIndex = Array.IndexOf(subOptions, step.subType);
                        if (subIndex < 0) subIndex = 0;
                        
                        int newSubIndex = EditorGUILayout.Popup("Sub Type", subIndex, subOptions);
                        step.subType = subOptions[newSubIndex];
                    }
                    else
                    {
                        step.subType = step.taskType;
                        EditorGUILayout.LabelField("Sub Type", step.subType);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Please link TaskDatabase to select SubTypes!", MessageType.Warning);
                    step.subType = EditorGUILayout.TextField("Sub Type (Manual)", step.subType);
                }

                EditorGUILayout.Space(10);

                // --- 文档配置区 (Input / Success / Fail) ---
                DrawDocumentGroup("📥 Input Document", ref step.inputDocName, ref step.inputIcon, ref step.inputDetailType, ref step.inputDetailText, ref step.inputDetailImage);
                
                EditorGUILayout.Space(5);
                DrawDocumentGroup("✅ Success Feedback", ref step.successDocName, ref step.successIcon, ref step.successDetailType, ref step.successDetailText, ref step.successDetailImage);
                
                EditorGUILayout.Space(5);
                DrawDocumentGroup("❌ Fail Feedback", ref step.failDocName, ref step.failIcon, ref step.failDetailType, ref step.failDetailText, ref step.failDetailImage);

                // --- 额外输出文档 (用于 Check Station) ---
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("--- Extra Outputs (For Check Station) ---", EditorStyles.boldLabel);
                
                DrawDocumentGroup("📄 Another Doc 1", ref step.anotherDoc1Name, ref step.anotherDoc1Icon, ref step.anotherDoc1DetailType, ref step.anotherDoc1Text, ref step.anotherDoc1Image);
                EditorGUILayout.Space(2);
                DrawDocumentGroup("📄 Another Doc 2", ref step.anotherDoc2Name, ref step.anotherDoc2Icon, ref step.anotherDoc2DetailType, ref step.anotherDoc2Text, ref step.anotherDoc2Image);

                EditorGUILayout.Space(10);
                
                // --- 结尾设置 ---
                step.isFinalStep = EditorGUILayout.Toggle("Is Final Step", step.isFinalStep);

                // ============================================================
                // ⭐ 新增功能：自动连接到下一步 (Auto Link)
                // ============================================================
                if (s < c.steps.Count - 1)
                {
                    EditorGUILayout.Space(10);
                    GUI.backgroundColor = Color.cyan; 
                    if (GUILayout.Button("⬇️ Auto Link Success to Next Step Input ⬇️", GUILayout.Height(25)))
                    {
                        LevelStep nextStep = c.steps[s + 1];
                        
                        nextStep.inputDocName = step.successDocName;
                        nextStep.inputIcon = step.successIcon;
                        nextStep.inputDetailType = step.successDetailType;
                        nextStep.inputDetailText = step.successDetailText;
                        nextStep.inputDetailImage = step.successDetailImage;

                        Debug.Log($"Linked Step {s+1} Output to Step {s+2} Input!");
                        GUI.FocusControl(null); 
                    }
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.Space();
                // --- 移除当前步骤按钮 ---
                if (GUILayout.Button("Remove Step", GUILayout.Width(100)))
                {
                    c.steps.RemoveAt(s);
                    break;
                }

                c.steps[s] = step;
                EditorGUILayout.EndVertical(); // End Step Box
                EditorGUILayout.Space(10);
            }

            // --- 添加新步骤按钮 ---
            if (GUILayout.Button("+ Add Step", GUILayout.Height(25)))
            {
                c.steps.Add(new LevelStep());
            }

            EditorGUILayout.Space();
            
            // --- 移除当前顾客按钮 ---
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Remove Customer"))
            {
                data.customers.RemoveAt(i);
                break;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical(); // End Customer Box
            EditorGUILayout.Space(20);
        }

        // --- 添加新顾客按钮 ---
        if (GUILayout.Button("Add New Customer", GUILayout.Height(40)))
        {
            data.customers.Add(new LevelCustomer());
        }

        // --- 标记脏数据 (保存修改) ---
        if (GUI.changed)
        {
            EditorUtility.SetDirty(data);
        }
    }

    // ============================================================
    // 辅助函数：绘制文档组 (也改大了)
    // ============================================================
    private void DrawDocumentGroup(string label, ref string name, ref Sprite icon, ref DetailType type, ref string text, ref Sprite detailImg)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);

        // ⭐ 改大：Doc Name
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Doc Name:", GUILayout.Width(80));
        name = EditorGUILayout.TextField(name, GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();

        icon = (Sprite)EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);
        
        type = (DetailType)EditorGUILayout.EnumPopup("Detail Type", type);

        if (type == DetailType.Text)
        {
            EditorGUILayout.LabelField("Content Text:");
            EditorStyles.textField.wordWrap = true;
            // ⭐ 改大：Doc Content Text
            text = EditorGUILayout.TextArea(text, GUILayout.Height(60));
        }
        else
        {
            detailImg = (Sprite)EditorGUILayout.ObjectField("Detail Image", detailImg, typeof(Sprite), false);
        }
        EditorGUILayout.EndVertical();
    }

    // ============================================================
    // 辅助函数：从 Database 获取子类型
    // ============================================================
    private string[] GetSubOptions(TaskDatabase db, string taskType)
    {
        if (db == null) return new string[0];

        switch (taskType)
        {
            case "kill": return db.virusTypes;
            case "check": return db.checkTypes;
            case "protect": return db.protectTypes;
            case "encrypt": return db.encryptTypes;
            case "decrypt": return db.decryptTypes;
        }
        return new string[0];
    }
}