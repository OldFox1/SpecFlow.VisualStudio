﻿using System;
using System.Linq;
using System.Collections.Generic;
using TechTalk.SpecFlow.Utils;
using TechTalk.SpecFlow.Vs2010Integration.Bindings;

namespace TechTalk.SpecFlow.Vs2010Integration.StepSuggestions
{
    public interface IStepSuggestionGroup<TNativeSuggestionItem>
    {
        IEnumerable<IStepSuggestion<TNativeSuggestionItem>> Suggestions { get; }
    }

    public class BoundStepSuggestions<TNativeSuggestionItem> : IStepSuggestionGroup<TNativeSuggestionItem>
    {
        private readonly StepSuggestionList<TNativeSuggestionItem> suggestions;
        public IEnumerable<IStepSuggestion<TNativeSuggestionItem>> Suggestions { get { return suggestions; } }

        public TNativeSuggestionItem NativeSuggestionItem { get; private set; }

        public StepBinding StepBinding { get; private set; }
        public BindingType BindingType { get; set; }

        public BoundStepSuggestions(BindingType bindingType, INativeSuggestionItemFactory<TNativeSuggestionItem> nativeSuggestionItemFactory)
        {
            StepBinding = null;
            BindingType = bindingType;
            NativeSuggestionItem = nativeSuggestionItemFactory.Create("[unbound steps]", "...", 0, this, null);
            suggestions = new StepSuggestionList<TNativeSuggestionItem>(nativeSuggestionItemFactory);
        }

        public BoundStepSuggestions(StepBinding stepBinding, INativeSuggestionItemFactory<TNativeSuggestionItem> nativeSuggestionItemFactory)
        {
            if (stepBinding == null) throw new ArgumentNullException("stepBinding");

            StepBinding = stepBinding;
            BindingType = stepBinding.BindingType;
            string suggestionText = GetSuggestionText(stepBinding);
            NativeSuggestionItem = nativeSuggestionItemFactory.Create(suggestionText, GetInsertionText(StepBinding), 0, this, BindingType.ToString().Substring(0, 1) + "-b");
            suggestions = new StepSuggestionList<TNativeSuggestionItem>(nativeSuggestionItemFactory);
        }

        private string GetSuggestionText(StepBinding stepBinding)
        {
            string suggestionTextBase = stepBinding.Regex == null ? "[...]" :
                "[" + RegexSampler.GetRegexSample(stepBinding.Regex.ToString(), stepBinding.Method.Parameters.Select(p => p.ParameterName).ToArray()) + "]";

            return string.Format("{0} -> {1}", suggestionTextBase, stepBinding.Method.ShortDisplayText);
        }

        private string GetInsertionText(StepBinding stepBinding)
        {
            if (stepBinding.Regex == null)
                return "...";

            var paramNames = stepBinding.Method.Parameters.Select(p => p.ParameterName);
            return RegexSampler.GetRegexSample(stepBinding.Regex.ToString(), paramNames.ToArray());
        }

        public void AddSuggestion(IStepSuggestion<TNativeSuggestionItem> stepSuggestion)
        {
            suggestions.Add(stepSuggestion);
            stepSuggestion.MatchGroups.Add(this);
        }

        public void RemoveSuggestion(IStepSuggestion<TNativeSuggestionItem> stepSuggestion)
        {
            suggestions.Remove(stepSuggestion);
            stepSuggestion.MatchGroups.Remove(this);
        }
    }
}