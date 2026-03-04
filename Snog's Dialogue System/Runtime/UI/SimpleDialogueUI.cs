using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SnogDialogue.Runtime
{
    public sealed class SimpleDialogueUI : MonoBehaviour, IDialogueUI
    {
        [Header("Panels")]
        [SerializeField] private GameObject linePanel;

        [SerializeField] private GameObject choicesPanel;

        [Header("Line UI")]
        [SerializeField] private TMP_Text lineText;

        [SerializeField] private Button continueButton;

        [SerializeField] private float charactersPerSecond = 40f;

        [Header("Choices UI")]
        [SerializeField] private Transform choicesContainer;

        [SerializeField] private Button choiceButtonPrefab;

        [Header("Typewriter Settings")]
        [SerializeField] private Typewriter typewriter;

        private Action continueRequested;
        private Action<int> choiceSelected;

        private Coroutine typeCoroutine;
        private bool isTyping;
        private string fullLine;

        private readonly List<Button> spawnedChoiceButtons = new List<Button>();

        private void Awake()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            Hide();
        }

        public void ShowLine(string text, Action onContinueRequested)
        {
            continueRequested = onContinueRequested;

            linePanel.SetActive(true);
            choicesPanel.SetActive(false);

            ClearChoices();

            fullLine = text ?? string.Empty;

            if (typewriter != null)
            {
                typewriter.Play(fullLine, null);
            }
            else
            {
                if (lineText != null)
                {
                    lineText.text = fullLine;
                }
            }
        }

        public void ShowChoices(IReadOnlyList<ChoiceUIEntry> choices, Action<int> onChoiceSelected)
        {
            choiceSelected = onChoiceSelected;

            linePanel.SetActive(false);
            choicesPanel.SetActive(true);

            ClearChoices();

            for (int i = 0; i < choices.Count; i++)
            {
                int index = i;

                Button btn = Instantiate(choiceButtonPrefab, choicesContainer);
                spawnedChoiceButtons.Add(btn);

                ChoiceUIEntry entry = choices[i];

                btn.interactable = entry.Interactable;

                TMP_Text label = btn.GetComponentInChildren<TMP_Text>();

                if (label != null)
                {
                    label.text = entry.Text;

                    if (!entry.Interactable)
                    {
                        label.alpha = 0.5f;
                    }
                    else
                    {
                        label.alpha = 1f;
                    }
                }

                if (entry.Interactable)
                {
                    btn.onClick.AddListener(() =>
                    {
                        choiceSelected?.Invoke(index);
                    });
                }
            }
        }

        public void Hide()
        {
            if (typeCoroutine != null)
            {
                StopCoroutine(typeCoroutine);
                typeCoroutine = null;
            }

            isTyping = false;
            fullLine = string.Empty;

            continueRequested = null;
            choiceSelected = null;

            ClearChoices();

            if (linePanel != null)
            {
                linePanel.SetActive(false);
            }

            if (choicesPanel != null)
            {
                choicesPanel.SetActive(false);
            }

            if (typewriter != null)
            {
                typewriter.StopTyping();
            }
        }

        private IEnumerator TypeLine(string text)
        {
            isTyping = true;

            if (lineText != null)
            {
                lineText.text = string.Empty;
            }

            float delay = 1f / Mathf.Max(1f, charactersPerSecond);

            for (int i = 0; i < text.Length; i++)
            {
                if (lineText != null)
                {
                    lineText.text = text.Substring(0, i + 1);
                }

                yield return new WaitForSeconds(delay);
            }

            isTyping = false;
            typeCoroutine = null;
        }

        private void OnContinueClicked()
        {
            if (typewriter != null && typewriter.IsTyping)
            {
                typewriter.SkipToEnd();
                return;
            }

            continueRequested?.Invoke();
        }

        private void ClearChoices()
        {
            for (int i = 0; i < spawnedChoiceButtons.Count; i++)
            {
                if (spawnedChoiceButtons[i] != null)
                {
                    Destroy(spawnedChoiceButtons[i].gameObject);
                }
            }

            spawnedChoiceButtons.Clear();
        }
    }
}
