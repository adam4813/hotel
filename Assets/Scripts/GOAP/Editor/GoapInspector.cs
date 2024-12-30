using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GoapInspector : EditorWindow
{
    private readonly Dictionary<string, bool> _foldouts = new();
    private bool goalsFoldout;
    private bool actionsFoldout;
    private bool beliefsFoldout = true;

    [MenuItem("Window/GOAP Inspector")]
    public static void ShowWindow()
    {
        GetWindow(typeof(GoapInspector));
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("GOAP Inspector", EditorStyles.boldLabel);
        if (!GameManager.Instance) return;

        EditorGUILayout.LabelField("Select a GOAP Agent to inspect");
        var agentIndex =
            EditorGUILayout.Popup("Agent", 0, GameManager.Instance.Agents.Select(agent => agent.name).ToArray());

        if (agentIndex >= GameManager.Instance.Agents.Count || agentIndex < 0)
        {
            return;
        }

        var agent = GameManager.Instance.Agents[agentIndex];

        RenderAgentGoals(agent);
        RenderAgentActions(agent);
        RenderAgentBeliefs(agent);
        RenderAgentActionPlan(agent);
    }

    private void RenderAgentGoals(GoapAgent agent)
    {
        goalsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(goalsFoldout, "Agent Goals");
        if (goalsFoldout)
        {
            EditorGUI.indentLevel++;
            var goalsByPriority = agent.Goals.OrderByDescending(goal => goal.Priority);
            foreach (var goal in goalsByPriority)
            {
                EditorGUILayout.LabelField($"({goal.Priority}) Name: {goal.Name}");
                _foldouts[goal.Name] =
                    EditorGUILayout.Foldout(_foldouts.GetValueOrDefault(goal.Name, false),
                        "Desired Effects");

                if (!_foldouts[goal.Name]) continue;

                EditorGUI.indentLevel++;
                foreach (var effect in goal.DesiredEffects)
                {
                    EditorGUILayout.LabelField(effect.Name);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
    }

    private void RenderAgentActions(GoapAgent agent)
    {
        actionsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(actionsFoldout, "Agent Actions");
        if (actionsFoldout)
        {
            EditorGUI.indentLevel++;
            foreach (var action in agent.Actions)
            {
                _foldouts[action.Name] = EditorGUILayout.Foldout(_foldouts.GetValueOrDefault(action.Name, false), action.Name);

                if (!_foldouts[action.Name]) continue;

                GUI.enabled = false;
                EditorGUILayout.LabelField("Preconditions");
                EditorGUI.indentLevel++;
                foreach (var precondition in action.Preconditions)
                {
                    EditorGUILayout.ToggleLeft(precondition.Name, precondition.Evaluate());
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("Effects");;
                EditorGUI.indentLevel++;
                foreach (var effect in action.Effects)
                {
                    EditorGUILayout.ToggleLeft(effect.Name, effect.Evaluate());
                }
                EditorGUI.indentLevel--;
                GUI.enabled = true;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
    }

    private static void RenderAgentActionPlan(GoapAgent agent)
    {
        if (agent.ActionPlan == null) return;

        EditorGUILayout.LabelField("Action Plan");
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField($"Current Goal: {agent.CurrentGoal.Name}");
        foreach (var action in agent.ActionPlan.Actions)
        {
            EditorGUILayout.LabelField(action.Name);
        }

        EditorGUI.indentLevel--;
    }

    private void RenderAgentBeliefs(GoapAgent agent)
    {
        beliefsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(beliefsFoldout, "Agent Beliefs");
        if (beliefsFoldout)
        {
            GUI.enabled = false;
            EditorGUI.indentLevel++;
            foreach (var belief in agent.Beliefs)
            {
                EditorGUILayout.ToggleLeft(belief.Key, belief.Value.Evaluate());
            }

            EditorGUI.indentLevel--;

            GUI.enabled = true;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
    }
}