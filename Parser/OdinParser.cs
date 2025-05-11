using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.VisualStudio.Text;

namespace OdinLangIntegrationVS.Parser
{
    internal class OdinParser
    {
        private static readonly ImmutableHashSet<string> Keywords = ImmutableHashSet.Create("asm", "auto_cast", "bit_set", "break", "case", "cast", "context",
            "continue", "defer", "distinct", "do", "dynamic", "else", "enum",
            "fallthrough", "for", "foreign", "if", "import", "in", "map", "not_in", "or_else", "or_return", "package", "proc", "return",
            "struct", "switch", "transmute", "typeid", "union", "using", "when", "where");

        private static readonly ImmutableHashSet<string> Operators_triples =
            ImmutableHashSet.Create("---", "||=", "&&=", "..=", "..<", ">>=", "<<=", "&~=", "%%=");

        private static readonly ImmutableHashSet<string> Operator_doubles = ImmutableHashSet.Create("%%", "&~", "<<", ">>", "&&", "||", "+=", "-=", "*=",
            "/=", "%=", "&=", "|=", "~=", "->", "==", "!=", "<=", ">=", "..", "++", "--");

        private static readonly ImmutableHashSet<string> BuiltinTypes = ImmutableHashSet.Create("byte",
"bool",
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
                   (ch >= '{' && ch >= '~');
        }

        private static bool IsSingleCharOperator(char ch)
        {
            return (ch >= ':' && ch <= '?') ||
                   ch == '^' || ch == '~' ||
                   (ch <= '/' && ch >= '*') ||
                   ch == '!' || ch == '%' || ch == '&';
        }

        public IEnumerable<Token> Tokenize(string source, bool retPlaintextOrWS)
        {
            var tokens = new List<Token>();

            //Debug.WriteLine("Tokenizing:");
            //Debug.Write(source);

            var oobChar = ' ';

            Func<int, char> getChar = (int idx) =>
            {
                if (idx < 0 || idx >= source.Length) return oobChar;
                return source[idx];
            };

            int startOfIdent = -1;
            int startOfSingleLineComment = -1;
            int startOfMultiLineComment = -1;
            int multiLineCommentBalance = 0;

            for (int i = 0; i < source.Length; ++i)
            {
                char ch = source[i];
                char prevCh = getChar(i - 1);
                char nextCh = getChar(i + 1);

                // Reset on newline
                if (ch == '\r' || ch == '\n')
                {
                    startOfIdent = -1;

                    if (startOfSingleLineComment != -1)
                    {
                        tokens.Add(new Token(new Span(startOfSingleLineComment, i-startOfSingleLineComment+1), TokenType.COMMENT));
                        startOfSingleLineComment = -1;
                    }

                    continue;
                }

                if (ch == '/' && nextCh == '*')
                {
                    if (multiLineCommentBalance == 0)
                    {
                        startOfMultiLineComment = i;
                    }

                    ++multiLineCommentBalance;

                } else if (ch == '*' && nextCh == '/')
                {
                    // ERROR: Not in a comment, this is illegal symbol
                    if (multiLineCommentBalance == 0)
                    {
                        continue;
                    }

                    multiLineCommentBalance = Math.Max(multiLineCommentBalance-1, 0);

                    if (multiLineCommentBalance == 0)
                    {
                        tokens.Add(new Token(new Span(startOfMultiLineComment, i-startOfMultiLineComment+1), TokenType.COMMENT));
                    }
                }

                if (startOfSingleLineComment != -1 || multiLineCommentBalance > 0) continue;

                if (ch == '/' && nextCh == '/')
                {
                    startOfSingleLineComment = i;
                    continue;
                }

                if (char.IsWhiteSpace(ch))
                {
                    startOfIdent = -1;
                }
                else
                {
                    if (IsPunctuation(ch))
                    {
                        if (Operators_triples.Contains(string.Concat(ch, nextCh, getChar(i + 2))))
                        {
                            tokens.Add(new Token(new Span(i, 3), TokenType.OPERATOR));
                        }
                        else if (Operator_doubles.Contains(string.Concat(ch, nextCh)))
                        {
                            tokens.Add(new Token(new Span(i, 2), TokenType.OPERATOR));
                        }
                        else if (IsSingleCharOperator(ch))
                        {
                            tokens.Add(new Token(new Span(i, 1), TokenType.OPERATOR));
                        }
                        else
                        {
                            tokens.Add(new Token(new Span(i, 1), TokenType.PUNCTUATION));
                        }
                    }
                    else
                    {
                        bool isPrevChNonIdent = char.IsWhiteSpace(prevCh) || IsPunctuation(prevCh);

                        bool isNextChNonIdent = char.IsWhiteSpace(nextCh) || IsPunctuation(nextCh);

                        if (isPrevChNonIdent)
                        {
                            startOfIdent = i;
                        }

                        if (isNextChNonIdent)
                        {
                            if (startOfIdent != -1)
                            {
                                Span identSpan = new Span(startOfIdent, i - startOfIdent + 1);
                                string ident = source.Substring(identSpan.Start, identSpan.Length);
                                Debug.WriteLine("Ident:{0}-{1}", identSpan.Start, identSpan.End);
                                tokens.Add(new Token(identSpan,
                                    Keywords.Contains(ident) ? TokenType.KEYWORD :
                                    BuiltinTypes.Contains(ident) ? TokenType.TYPE :
                                    TokenType.IDENTIFIER));
                            }
                        }
                    }
                }
            }

            return tokens;
        }
    }
}
