// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

/// <summary>This namespace contains types and modules for generating and formatting text with F#</summary>
namespace Microsoft.FSharp.Core

open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open System
open System.IO
open System.Text

/// <summary>Type of a formatting expression.</summary>
/// <typeparam name="Printer">Function type generated by printf.</typeparam>
/// <typeparam name="State">Type argument passed to %a formatters</typeparam>
/// <typeparam name="Residue">Value generated by the overall printf action (e.g. sprint generates a string)</typeparam>
/// <typeparam name="Result">Value generated after post processing (e.g. failwithf generates a string internally then raises an exception)</typeparam>
type PrintfFormat<'Printer,'State,'Residue,'Result> =
    /// <summary>Construct a format string </summary>
    /// <param name="value">The input string.</param>
    /// <returns>The PrintfFormat containing the formatted result.</returns>
    new : value:string -> PrintfFormat<'Printer,'State,'Residue,'Result>
    /// <summary>The raw text of the format string.</summary>
    member Value : string
    
/// <summary>Type of a formatting expression.</summary>
/// <typeparam name="Printer">Function type generated by printf.</typeparam>
/// <typeparam name="State">Type argument passed to %a formatters</typeparam>
/// <typeparam name="Residue">Value generated by the overall printf action (e.g. sprint generates a string)</typeparam>
/// <typeparam name="Result">Value generated after post processing (e.g. failwithf generates a string internally then raises an exception)</typeparam>
/// <typeparam name="Tuple">Tuple of values generated by scan or match.</typeparam>
type PrintfFormat<'Printer,'State,'Residue,'Result,'Tuple> = 
    inherit PrintfFormat<'Printer,'State,'Residue,'Result>
    /// <summary>Construct a format string</summary>
    /// <param name="value">The input string.</param>
    /// <returns>The created format string.</returns>
    new: value:string -> PrintfFormat<'Printer,'State,'Residue,'Result,'Tuple>

/// <summary>Type of a formatting expression.</summary>
/// <typeparam name="Printer">Function type generated by printf.</typeparam>
/// <typeparam name="State">Type argument passed to %a formatters</typeparam>
/// <typeparam name="Residue">Value generated by the overall printf action (e.g. sprint generates a string)</typeparam>
/// <typeparam name="Result">Value generated after post processing (e.g. failwithf generates a string internally then raises an exception)</typeparam>
type Format<'Printer,'State,'Residue,'Result> = PrintfFormat<'Printer,'State,'Residue,'Result>

/// <summary>Type of a formatting expression.</summary>
/// <typeparam name="Printer">Function type generated by printf.</typeparam>
/// <typeparam name="State">Type argument passed to %a formatters</typeparam>
/// <typeparam name="Residue">Value generated by the overall printf action (e.g. sprint generates a string)</typeparam>
/// <typeparam name="Result">Value generated after post processing (e.g. failwithf generates a string internally then raises an exception)</typeparam>
/// <typeparam name="Tuple">Tuple of values generated by scan or match.</typeparam>
type Format<'Printer,'State,'Residue,'Result,'Tuple> = PrintfFormat<'Printer,'State,'Residue,'Result,'Tuple>

/// <summary>Extensible printf-style formatting for numbers and other datatypes</summary>
///
/// <remarks>Format specifications are strings with "%" markers indicating format 
/// placeholders. Format placeholders consist of:
///  <c>
///    %[flags][width][.precision][type]
///  </c>
/// where the type is interpreted as follows:
///  <c>
///     %b:         bool, formatted as "true" or "false"
///     %s:         string, formatted as its unescaped contents
///     %c:         character literal
///     %d, %i:     any basic integer type formatted as a decimal integer, signed if the basic integer type is signed.
///     %u:         any basic integer type formatted as an unsigned decimal integer
///     %x, %X, %o: any basic integer type formatted as an unsigned hexadecimal 
///                 (a-f)/Hexadecimal (A-F)/Octal integer
/// 
///     %e, %E, %f, %F, %g, %G: 
///                 any basic floating point type (float,float32) formatted
///                 using a C-style floating point format specifications, i.e
/// 
///     %e, %E: Signed value having the form [-]d.dddde[sign]ddd where 
///                 d is a single decimal digit, dddd is one or more decimal
///                 digits, ddd is exactly three decimal digits, and sign 
///                 is + or -
/// 
///     %f:     Signed value having the form [-]dddd.dddd, where dddd is one
///                 or more decimal digits. The number of digits before the 
///                 decimal point depends on the magnitude of the number, and 
///                 the number of digits after the decimal point depends on 
///                 the requested precision.
/// 
///     %g, %G: Signed value printed in f or e format, whichever is 
///                 more compact for the given value and precision.
/// 
/// 
///    %M:      System.Decimal value
/// 
///    %O:      Any value, printed by boxing the object and using it's ToString method(s)
/// 
///    %A:      Any value, printed with the default layout settings 
/// 
///    %a:      A general format specifier, requires two arguments:
///                 (1) a function which accepts two arguments:
///                     (a) a context parameter of the appropriate type for the
///                         given formatting function (e.g. an #System.IO.TextWriter)
///                     (b) a value to print
///                         and which either outputs or returns appropriate text.
/// 
///                 (2) the particular value to print
/// 
/// 
///    %t:      A general format specifier, requires one argument:
///                 (1) a function which accepts a context parameter of the
///                     appropriate type for the given formatting function (e.g. 
///                     an System.IO.TextWriter)and which either outputs or returns 
///                     appropriate text.
///
///  Basic integer types are:
///     byte,sbyte,int16,uint16,int32,uint32,int64,uint64,nativeint,unativeint
///  Basic floating point types are:
///     float, float32
/// </c>
/// The optional width is an integer indicating the minimal width of the
/// result. For instance, %6d prints an integer, prefixing it with spaces
/// to fill at least 6 characters. If width is '*', then an extra integer
/// argument is taken to specify the corresponding width.
/// <c>
///     any number
///     '*': 
/// </c>
/// Valid flags are:
/// <c>
///     0: add zeros instead of spaces to make up the required width
///     '-': left justify the result within the width specified
///     '+': add a '+' character if the number is positive (to match a '-' sign 
///          for negatives)
///     ' ': add an extra space if the number is positive (to match a '-' 
///              sign for negatives)
/// </c>
/// The printf '#' flag is invalid and a compile-time error will be reported if it is used.</remarks>
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Printf = 

    /// <summary>Represents a statically-analyzed format associated with writing to a <c>System.Text.StringBuilder</c>. The first type parameter indicates the
    /// arguments of the format operation and the last the overall return type.</summary>
    type BuilderFormat<'T,'Result>    = Format<'T, StringBuilder, unit, 'Result>
    /// <summary>Represents a statically-analyzed format when formatting builds a string. The first type parameter indicates the
    /// arguments of the format operation and the last the overall return type.</summary>
    type StringFormat<'T,'Result>     = Format<'T, unit, string, 'Result>
    /// <summary>Represents a statically-analyzed format associated with writing to a <c>System.IO.TextWriter</c>. The first type parameter indicates the
    /// arguments of the format operation and the last the overall return type.</summary>
    type TextWriterFormat<'T,'Result> = Format<'T, TextWriter, unit, 'Result>

    /// <summary>Represents a statically-analyzed format associated with writing to a <c>System.Text.StringBuilder</c>. The type parameter indicates the
    /// arguments and return type of the format operation.</summary>
    type BuilderFormat<'T>     = BuilderFormat<'T,unit>
    /// <summary>Represents a statically-analyzed format when formatting builds a string. The type parameter indicates the
    /// arguments and return type of the format operation.</summary>
    type StringFormat<'T>      = StringFormat<'T,string>
    /// <summary>Represents a statically-analyzed format associated with writing to a <c>System.IO.TextWriter</c>. The type parameter indicates the
    /// arguments and return type of the format operation.</summary>
    type TextWriterFormat<'T>  = TextWriterFormat<'T,unit>



    /// <summary>Print to a <c>System.Text.StringBuilder</c></summary>
    /// <param name="builder">The StringBuilder to print to.</param>
    /// <param name="format">The input formatter.</param>
    /// <returns>The return type and arguments of the formatter.</returns>
    [<CompiledName("PrintFormatToStringBuilder")>]
    val bprintf : builder:StringBuilder -> format:BuilderFormat<'T> -> 'T

    /// <summary>Print to a text writer.</summary>
    /// <param name="textWriter">The TextWriter to print to.</param>
    /// <param name="format">The input formatter.</param>
    /// <returns>The return type and arguments of the formatter.</returns>
    [<CompiledName("PrintFormatToTextWriter")>]
    val fprintf : textWriter:TextWriter -> format:TextWriterFormat<'T> -> 'T

    /// <summary>Print to a text writer, adding a newline</summary>
    /// <param name="textWriter">The TextWriter to print to.</param>
    /// <param name="format">The input formatter.</param>
    /// <returns>The return type and arguments of the formatter.</returns>
    [<CompiledName("PrintFormatLineToTextWriter")>]
    val fprintfn : textWriter:TextWriter -> format:TextWriterFormat<'T> -> 'T

#if !FX_NO_SYSTEM_CONSOLE
    /// <summary>Formatted printing to stderr</summary>
    /// <param name="format">The input formatter.</param>
    /// <returns>The return type and arguments of the formatter.</returns>
    [<CompiledName("PrintFormatToError")>]
    val eprintf :                 format:TextWriterFormat<'T> -> 'T

    /// <summary>Formatted printing to stderr, adding a newline </summary>
    /// <param name="format">The input formatter.</param>
    /// <returns>The return type and arguments of the formatter.</returns>
    [<CompiledName("PrintFormatLineToError")>]
    val eprintfn :                format:TextWriterFormat<'T> -> 'T

    /// <summary>Formatted printing to stdout</summary>
    /// <param name="format">The input formatter.</param>
    /// <returns>The return type and arguments of the formatter.</returns>
    [<CompiledName("PrintFormat")>]
    val printf  :                 format:TextWriterFormat<'T> -> 'T

    /// <summary>Formatted printing to stdout, adding a newline.</summary>
    /// <param name="format">The input formatter.</param>
    /// <returns>The return type and arguments of the formatter.</returns>
    [<CompiledName("PrintFormatLine")>]
    val printfn  :                format:TextWriterFormat<'T> -> 'T
#endif
    /// <summary>Print to a string via an internal string buffer and return 
    /// the result as a string. Helper printers must return strings.</summary>
    /// <param name="format">The input formatter.</param>
    /// <returns>The formatted string.</returns>
    [<CompiledName("PrintFormatToStringThen")>]
    val sprintf :                 format:StringFormat<'T> -> 'T

    /// <summary>bprintf, but call the given 'final' function to generate the result.
    /// See <c>kprintf</c>.</summary>
    /// <param name="continuation">The function called after formatting to generate the format result.</param>
    /// <param name="builder">The input StringBuilder.</param>
    /// <param name="format">The input formatter.</param>
    /// <returns>The arguments of the formatter.</returns>
    [<CompiledName("PrintFormatToStringBuilderThen")>]
    val kbprintf : continuation:(unit -> 'Result)   -> builder:StringBuilder ->    format:BuilderFormat<'T,'Result> -> 'T

    /// <summary>fprintf, but call the given 'final' function to generate the result.
    /// See <c>kprintf</c>.</summary>
    /// <param name="continuation">The function called after formatting to generate the format result.</param>
    /// <param name="textWriter">The input TextWriter.</param>
    /// <param name="format">The input formatter.</param>
    /// <returns>The arguments of the formatter.</returns>
    [<CompiledName("PrintFormatToTextWriterThen")>]
    val kfprintf : continuation:(unit -> 'Result)   -> textWriter:TextWriter -> format:TextWriterFormat<'T,'Result> -> 'T

    /// <summary>printf, but call the given 'final' function to generate the result.
    /// For example, these let the printing force a flush after all output has 
    /// been entered onto the channel, but not before. </summary>
    /// <param name="continuation">The function called after formatting to generate the format result.</param>
    /// <param name="format">The input formatter.</param>
    /// <returns>The arguments of the formatter.</returns>
    [<CompiledName("PrintFormatThen")>]
    val kprintf  : continuation:(string -> 'Result) ->                format:StringFormat<'T,'Result> -> 'T

    /// <summary>sprintf, but call the given 'final' function to generate the result.
    /// See <c>kprintf</c>.</summary>
    /// <param name="continuation">The function called to generate a result from the formatted string.</param>
    /// <param name="format">The input formatter.</param>
    /// <returns>The arguments of the formatter.</returns>
    [<CompiledName("PrintFormatToStringThen")>]
    val ksprintf : continuation:(string -> 'Result) ->                format:StringFormat<'T,'Result>  -> 'T

    /// <summary>Print to a string buffer and raise an exception with the given
    /// result. Helper printers must return strings.</summary>
    /// <param name="format">The input formatter.</param>
    /// <returns>The arguments of the formatter.</returns>
    [<CompiledName("PrintFormatToStringThenFail")>]
    val failwithf: format:StringFormat<'T,'Result> -> 'T
