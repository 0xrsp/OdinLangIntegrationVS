using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Media3D;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI.OleComponentSupport;
using Microsoft.VisualStudio.Text;

namespace OdinLangIntegrationVS.Parser
{
    internal class OdinHLParser
    {
        private static readonly ImmutableHashSet<string> Keywords = ImmutableHashSet.Create("asm", "auto_cast", "bit_set", "break", "case", "cast", "context",
            "continue", "defer", "distinct", "do", "dynamic", "else", "enum",
            "fallthrough", "for", "foreign", "if", "import", "in", "map", "not_in", "or_else", "or_return", "package", "proc", "return",
            "struct", "switch", "transmute", "typeid", "union", "using", "when", "where");

        private static readonly ImmutableHashSet<string> OperatorsTriples =
            ImmutableHashSet.Create("---", "||=", "&&=", "..=", "..<", ">>=", "<<=", "&~=", "%%=");

        private static readonly ImmutableHashSet<string> OperatorDoubles = ImmutableHashSet.Create("%%", "&~", "<<", ">>", "&&", "||", "+=", "-=", "*=",
            "/=", "%=", "&=", "|=", "~=", "->", "==", "!=", "<=", ">=", "..", "++", "--");

        private static readonly ImmutableHashSet<string> BuiltinProcs = ImmutableHashSet.Create(
    "len",
    "cap",
    "size_of",
    "align_of",
    "offset_of",
    "offset_of_selector",
    "offset_of_member",
    "offset_of_by_string",
    "type_of",
    "type_info_of",
    "typeid_of",
    "swizzle",
    "complex",
    "quaternion",
    "real",
    "imag",
    "jmag",
    "kmag",
    "conj",
    "expand_values",
    "min",
    "max",
    "abs",
    "clamp",
    "soa_zip",
    "soa_unzip",
    "raw_data",
    "container_of",
    "init_global_temporary_allocator",
    "copy_slice",
    "copy_from_string",
    "unordered_remove",
    "ordered_remove",
    "remove_range",
    "pop",
    "pop_safe",
    "pop_front",
    "pop_front_safe",
    "delete_string",
    "delete_cstring",
    "delete_dynamic_array",
    "delete_slice",
    "delete_map",
    "new",
    "new_clone",
    "make_slice",
    "make_dynamic_array",
    "make_dynamic_array_len",
    "make_dynamic_array_len_cap",
    "make_map",
    "make_map_cap",
    "make_multi_pointer",
    "clear_map",
    "reserve_map",
    "shrink_map",
    "delete_key",
    "append_elem",
    "non_zero_append_elem",
    "append_elems",
    "non_zero_append_elems",
    "append_elem_string",
    "non_zero_append_elem_string",
    "append_string",
    "append_nothing",
    "inject_at_elem",
    "inject_at_elems",
    "inject_at_elem_string",
    "assign_at_elem",
    "assign_at_elems",
    "assign_at_elem_string",
    "clear_dynamic_array",
    "reserve_dynamic_array",
    "non_zero_reserve_dynamic_array",
    "resize_dynamic_array",
    "non_zero_resize_dynamic_array",
    "map_insert",
    "map_upsert",
    "map_entry",
    "card",
    "assert",
    "ensure",
    "panic",
    "unimplemented",
    "assert_contextless",
    "ensure_contextless",
    "panic_contextless",
    "unimplemented_contextless",
    "raw_soa_footer_slice",
    "raw_soa_footer_dynamic_array",
    "make_soa_aligned",
    "make_soa_slice",
    "make_soa_dynamic_array",
    "make_soa_dynamic_array_len",
    "make_soa_dynamic_array_len_cap",
    "resize_soa",
    "non_zero_resize_soa",
    "reserve_soa",
    "non_zero_reserve_soa",
    "append_soa_elem",
    "non_zero_append_soa_elem",
    "append_soa_elems",
    "non_zero_append_soa_elems",
    "unordered_remove_soa",
    "ordered_remove_soa",
    // PROC GROUPS
    "copy",
    "clear",
    "reserve",
    "non_zero_reserve",
    "resize",
    "non_zero_resize",
    "shrink",
    "free",
    "free_all",
    "delete",
    "make",
    "append",
    "non_zero_append",
    "inject_at",
    "assign_at",
    "make_soa",
    "append_soa",
    "delete_soa",
    "clear_soa"
);

        private static readonly ImmutableHashSet<string> BuiltinTypes = ImmutableHashSet.Create("byte", "bool",
"b8",
"b16",
"b32",
"b64",
"i8",
"u8",
"i16",
"u16",
"i32",
"u32",
"i64",
"u64",
"i128",
"u128",
"rune",
"f16",
"f32",
"f64",
"complex32",
"complex64",
"complex128",
"quaternion64",
"quaternion128",
"quaternion256",
"int",
"uint",
"uintptr",
"rawptr",
"string",
"cstring",
"typeid",
"any",
"i16le",
"u16le",
"i32le",
"u32le",
"i64le",
"u64le",
"i128le",
"u128le",
"i16be",
"u16be",
"i32be",
"u32be",
"i64be",
"u64be",
"i128be",
"u128be",
"f16le",
"f32le",
"f64le",
"f16be",
"f32be",
"f64be");

        private static bool IsPunctuation(char ch)
        {
            return (ch >= '!' && ch <= '/') ||
                   (ch >= ':' && ch <= '@') ||
                   (ch >= '[' && ch <= '^') ||
                   (ch >= '{' && ch >= '~') ||
                   ch == '`';
        }

        private static bool IsCharNumberStart(char ch, char nextCh)
        {
            return char.IsDigit(ch) || (ch == '.' && char.IsDigit(nextCh));
        }

        private static bool IsCharNumberAllowed(char ch)
        {
            return char.IsDigit(ch) ||
                (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F') // includes e, E for exp format & b for binary
                || ch == 'x' || ch == 'o'
                || ch == '_' || ch == '.';
        }

        private static bool IsSingleCharOperator(char ch)
        {
            return (ch >= ':' && ch <= '?') ||
                   ch == '^' || ch == '~' ||
                   (ch <= '/' && ch >= '*') ||
                   ch == '!' || ch == '%' || ch == '&';
        }

        private enum ParserMode
        {
            MULTILINE_COMMENT,
            SINGLELINE_COMMENT,
            STRING_LITERAL,
            RAW_STRING_LITERAL,
            CHAR_LITERAL,
            NUMBER,
            IDENT,
            NONE,
        }

        private static bool IsInStringLiteral(ParserMode mode)
        {
            return mode == ParserMode.STRING_LITERAL || mode == ParserMode.RAW_STRING_LITERAL || mode == ParserMode.CHAR_LITERAL;
        }

        public static bool SkipWhitespaceAndMatch(Func<int, char> getChar, int offset, int maxLength, string pattern)
        {
            char ch;
            while (char.IsWhiteSpace(ch = getChar(offset)) && offset < maxLength)
            {
                ++offset;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < pattern.Length; ++i)
            {
                sb.Append(getChar(offset + i));
            }

            return sb.ToString().Equals(pattern);
        }

        public IEnumerable<HLElement> Tokenize(ITextSnapshot snapshot, Span span, bool retPlaintextOrWS)
        {
            var tokens = new List<HLElement>();
            var source = snapshot.GetText();

            var oobChar = ' ';

            Func<int, char> getChar = (int idx) =>
            {
                if (idx < 0 || idx >= snapshot.Length) return oobChar;
                return source[idx];
            };

            ParserMode mode = ParserMode.NONE;
            int mark = -1;
            int balance = 0;

            for (int i = 0; i < snapshot.Length; ++i)
            {
                bool isInUpdateRange = i >= span.Start && i < span.End;
                char ch = source[i];
                char nextCh = getChar(i + 1);

                bool isNewLine = ch == '\n' || (ch == '\r' && nextCh == '\n');

                if (!IsInStringLiteral(mode))
                {
                    if (ch == '/' && nextCh == '*')
                    {
                        if (balance == 0)
                        {
                            mode = ParserMode.MULTILINE_COMMENT;
                            mark = i;
                        }

                        ++balance;
                    }
                    else if (ch == '*' && nextCh == '/')
                    {
                        // ERROR: End of multi line comment with no start, this is illegal
                        if (balance == 0)
                        {
                            continue;
                        }

                        /*
                        if (mode != ParserMode.MULTILINE_COMMENT)
                        {
                            Debug.WriteLine("Invalid parser state");
                            break;
                        }*/

                        balance = Math.Max(balance - 1, 0);

                        if (balance == 0)
                        {
                            mode = ParserMode.NONE;
                            tokens.Add(new HLElement(new Span(mark, i - mark + 2), HLElementType.COMMENT));
                        }
                    }
                }
                else
                {
                    if (isNewLine)
                    {
                        switch (mode)
                        {
                            case ParserMode.RAW_STRING_LITERAL:
                                // TODO: Raw string literals
                                break;
                            case ParserMode.CHAR_LITERAL:
                            case ParserMode.STRING_LITERAL:
                                // ERROR: Illegal unterminated char or string literal
                                break;
                        }
                    }
                    else if (ch == '\"' && mode == ParserMode.STRING_LITERAL)
                    {
                        tokens.Add(new HLElement(new Span(mark, i - mark + 1), HLElementType.STRING_LITERAL));
                        mode = ParserMode.NONE;

                    }
                    else if (ch == '\'' && mode == ParserMode.CHAR_LITERAL)
                    {
                        tokens.Add(new HLElement(new Span(mark, i - mark + 1), HLElementType.STRING_LITERAL));
                        mode = ParserMode.NONE;
                    }

                    continue;
                }

                if (isNewLine && balance > 0)
                {
                    tokens.Add(new HLElement(new Span(mark, i - mark), HLElementType.COMMENT));
                }

                if (!isInUpdateRange) continue;

                if (isNewLine)
                {
                    switch (mode)
                    {
                        case ParserMode.SINGLELINE_COMMENT:
                            tokens.Add(new HLElement(new Span(mark, i - mark + 1), HLElementType.COMMENT));
                            mode = ParserMode.NONE;
                            break;
                    }

                    continue;
                }

                // Inside comment
                if (mode == ParserMode.SINGLELINE_COMMENT || mode == ParserMode.MULTILINE_COMMENT) continue;

                if (mode == ParserMode.NUMBER)
                {
                    if (!IsCharNumberAllowed(ch))
                    {
                        tokens.Add(new HLElement(new Span(mark, i - mark), HLElementType.NUMBER));
                        mode = ParserMode.NONE;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (ch == '\"')
                {
                    mode = ParserMode.STRING_LITERAL;
                    mark = i;
                }
                else if (ch == '\'')
                {
                    mode = ParserMode.CHAR_LITERAL;
                    mark = i;
                }
                else if (ch == '/' && nextCh == '/')
                {
                    mark = i;
                    mode = ParserMode.SINGLELINE_COMMENT;
                }
                else if (IsCharNumberStart(ch, nextCh) && 
                    // Fix: idents can have digits
                    !(mode == ParserMode.IDENT && char.IsDigit(ch)))
                {
                    mark = i;
                    mode = ParserMode.NUMBER;
                }
                else if (IsPunctuation(ch))
                {
                    if (OperatorsTriples.Contains(string.Concat(ch, nextCh, getChar(i + 2))))
                    {
                        tokens.Add(new HLElement(new Span(i, 3), HLElementType.OPERATOR));
                        i += 2;
                    }
                    else if (OperatorDoubles.Contains(string.Concat(ch, nextCh)))
                    {
                        tokens.Add(new HLElement(new Span(i, 2), HLElementType.OPERATOR));
                        i++;
                    }
                    else if (IsSingleCharOperator(ch))
                    {
                        tokens.Add(new HLElement(new Span(i, 1), HLElementType.OPERATOR));
                    }
                    else
                    {
                        tokens.Add(new HLElement(new Span(i, 1), HLElementType.PUNCTUATION));
                    }

                    mode = ParserMode.NONE;
                }
                else if (char.IsWhiteSpace(ch))
                {
                    mode = ParserMode.NONE;
                }
                else
                {
                    if (mode != ParserMode.IDENT)
                    {
                        mark = i;
                        mode = ParserMode.IDENT;
                    }
                    else
                    {
                        // Next char wont be ident
                        if (IsCharNumberStart(nextCh, getChar(i + 2)) ||
                            char.IsWhiteSpace(nextCh) ||
                            IsPunctuation(nextCh) ||
                            nextCh == '/')
                        {
                            /*
                            if (mark == -1 && mode == ParserMode.IDENT)
                            {
                                Debug.WriteLine("Parser error");
                            }*/

                            Span identSpan = new Span(mark, i - mark + 1);
                            string ident = source.Substring(identSpan.Start, identSpan.Length);

                            tokens.Add(new HLElement(identSpan,
                                BuiltinProcs.Contains(ident) ? HLElementType.SYM_REF :
                                Keywords.Contains(ident) ? HLElementType.KEYWORD :
                                BuiltinTypes.Contains(ident) ? HLElementType.TYPE :

                                // Manual override as func ref
                                nextCh == '(' ? HLElementType.SYM_REF :

                                // Manual override as sym def
                                SkipWhitespaceAndMatch(getChar, i + 1, span.Start + span.Length, "::") ? HLElementType.SYM_DEF :

                                HLElementType.IDENT));

                        }
                    }
                }

            }

            return tokens;
        }
    }
}
