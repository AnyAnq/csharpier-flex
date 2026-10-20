using System.Globalization;
using AwesomeAssertions;
using CSharpier.Core;
using CSharpier.Core.CSharp;

namespace CSharpier.Tests;

internal sealed class FormattingStyleMergeTests
{
    [Test]
    [Arguments("// Field comment")]
    [Arguments("/// <summary>Field documentation</summary>")]
    public void Resharper_Field_Leading_Comments_Do_Not_Break_Attribute_Group(string comment)
    {
        var result = Format(
            $"class Test {{\n{comment}\n[SerializeField] private int field;\n}}",
            FormattingStyle.Resharper
        );

        result
            .Should()
            .Be($"class Test\n{{\n    {comment}\n    [SerializeField] private int field;\n}}\n");
    }

    [Test]
    [Arguments(FormattingStyle.Default, "()")]
    [Arguments(FormattingStyle.Default, "value")]
    [Arguments(FormattingStyle.Resharper, "()")]
    [Arguments(FormattingStyle.Resharper, "value")]
    public void Trailing_Block_Lambda_Keeps_Preceding_Arguments_Inline(
        FormattingStyle style,
        string parameters
    )
    {
        var result = Format(
            $"BindButton(button, callback: {parameters} => {{ HandleClick(); }});",
            style
        );

        result
            .Should()
            .Be($"BindButton(button, callback: {parameters} =>\n{{\n    HandleClick();\n}});\n");
    }

    [Test]
    [Arguments("if", "if ({0}) {{ Run(); }}")]
    [Arguments("while", "while ({0}) {{ Run(); }}")]
    [Arguments("while", "do {{ Run(); }} while ({0});")]
    [Arguments("switch", "switch ({0}) {{ default: break; }}")]
    public void Resharper_Control_Statements_Keep_Conditions_Inside_Parentheses(
        string keyword,
        string statement
    )
    {
        const string condition = "firstConditionIsLongEnough || secondConditionIsLongEnough";
        var result = Format(
            string.Format(CultureInfo.InvariantCulture, statement, condition),
            FormattingStyle.Resharper,
            55
        );

        result
            .Should()
            .Contain($"{keyword} (firstConditionIsLongEnough ||\n    secondConditionIsLongEnough)");
    }

    [Test]
    public void Default_Condition_Keeps_Upstream_Operator_And_Parenthesis_Layout()
    {
        var result = Format(
            "if (firstConditionIsLongEnough || secondConditionIsLongEnough) { Run(); }",
            FormattingStyle.Default,
            55
        );

        result
            .Should()
            .Contain("if (\n    firstConditionIsLongEnough\n    || secondConditionIsLongEnough\n)");
    }

    [Test]
    public void Resharper_Null_Coalescing_Operators_Stay_On_Preceding_Line()
    {
        var result = Format(
            "var result = firstVeryLongCandidateValue ?? secondVeryLongCandidateValue ?? thirdVeryLongCandidateValue;",
            FormattingStyle.Resharper,
            55
        );

        result.Should().Contain("firstVeryLongCandidateValue ??\n");
        result.Should().Contain("secondVeryLongCandidateValue ??\n");
    }

    [Test]
    public void Default_Trailing_Lambda_Preserves_Comment_Before_Arguments()
    {
        var result = Format(
            "BindButton(\n// target\nbutton, () => { HandleClick(); });",
            FormattingStyle.Default
        );

        result.Should().Contain("BindButton(\n    // target\n    button,");
        result.Should().Contain("HandleClick();");
    }

    [Test]
    public void Default_Trailing_Lambda_Chops_When_Arguments_Exceed_Width()
    {
        var result = Format(
            "BindButton(firstButtonWithALongName, secondButtonWithALongName, () => { HandleClick(); });",
            FormattingStyle.Default,
            55
        );

        result.Should().StartWith("BindButton(\n    firstButtonWithALongName,\n");
        result.Should().Contain("    secondButtonWithALongName,\n");
    }

    private static string Format(string code, FormattingStyle style, int width = 100)
    {
        var options = new CodeFormatterOptions
        {
            FormattingStyle = style,
            Width = width,
            EndOfLine = EndOfLine.LF,
        };
        var result = CSharpFormatter.Format(code, options);
        result.ErrorDiagnostics.Should().BeEmpty();
        CSharpFormatter.Format(result.Code, options).Code.Should().Be(result.Code);
        return result.Code;
    }
}
