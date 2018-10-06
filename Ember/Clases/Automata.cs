﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace Ember.Clases
{
    class Automata
    {
        public int VARIABLES;
        public int CONSTANTES;
        public int ASIGNACIONES;
        public int CONDICIONALES;
        public int LOOPS;
        public int COMENTARIOS;

        public StreamReader input; // Variable donde se va a almacenar todo el fichero
        public string filePath; // Variable donde se va a almacenar todo el fichero
        public string buffer; // Variable donde se va a almacenar todo el fichero
        public int forward;
        public int inicial;
        public int line; // Variable para llevar el control de la linea que se está analizando
        public string[] palabrasReservadas = {
            "main",
            "case",
            "class",
            "const",
            "default",
            "delete",
            "else",
            "enum",
            "false",
            "true",
            "if",
            "for",
            "while",
            "do",
            "new",
            "private",
            "protected",
            "switch",
            "try",
            "catch",
            "return",
            "public" 
        }; // arreglo de palabras reservadas
        public string[] tiposDeDato = {
            "char",
            "int",
            "long",
            "double",
            "string",
            "short",
            "bool",
        }; // arreglo de palabras reservadas
        public static int PARENTESISABIERTO = 1; // Paréntesis abierto
        public static int PARENTESISCERRADO = 2; // Paréntesis cerrado
        public static int OPERADORARITMETICO = 3; // Operador aritmético (*¿-/)
        public static int OPERADORRELACIONAL = 4; // Operador relacional (= <= >=)
        public static int ASIGNACION = 5; // Asignación (=)
        public static int IDENTIFICADOR = 6; // Identificador (nombre de variable)
        public static int NUMERONATURAL = 7; // Numero natural (0-9)
        public static int ERROR = 8; // Error
        public static int LLAVEABIERTA = 9; // Llave abierta ({)
        public static int LLAVECERRADA = 10; // Llave cerrada (})
        public static int FINDEDECLARACION = 11; // Punto y coma (;)
        public static int PALABRARESERVADA = 12; // Palabra Reservada
        public static int COMENTARIOMONOLINEA = 13; // Comentario Monolinea (//)
        public static int COMENTARIOMULTILINEA = 14; // Comentario Multilinea (/**/)
        public static int ENDOFFILE = 15; // Fin del fichero (~)
        public static int TIPODEDATO = 16; // Fin del fichero (~)

        public Token SetLexema()
        {
            int state = 0;
            char caracter;
            String lexema = "";
            while (true)
            {
                caracter = NextChar();
                switch (state)
                {
                    case 0:
                        switch (caracter)
                        {
                            case '\r':
                                break;
                            case '~':
                                return new Token(ENDOFFILE);
                            case '\t':
                                break;
                            case '\0':
                                Retract();
                                break;
                            case '\n':
                                line++;
                                break;
                            case ' ':
                                break;
                            case ';':
                                return new Token(FINDEDECLARACION);
                            case '(':
                                return new Token(PARENTESISABIERTO);
                            case ')':
                                return new Token(PARENTESISCERRADO);
                            case '{':
                                return new Token(LLAVEABIERTA);
                            case '}':
                                return new Token(LLAVECERRADA);
                            case '*':
                                return new Token(OPERADORARITMETICO, Char.ToString(caracter));
                            case '+':
                                return new Token(OPERADORARITMETICO, Char.ToString(caracter));
                            case '-':
                                return new Token(OPERADORARITMETICO, Char.ToString(caracter));
                            case '/':
                                state = 6;
                                break;
                            case '>':
                                state = 1;
                                lexema = lexema + Char.ToString(caracter);
                                break;
                            case '<':
                                state = 1;
                                lexema = lexema + Char.ToString(caracter);
                                break;
                            case '=':
                                state = 2;
                                break;
                            case '!':
                                state = 3;
                                break;
                            default:
                                if (IsLetter(caracter.ToString()) || caracter == '_')
                                {
                                    state = 5;
                                    lexema = lexema + Char.ToString(caracter);
                                    break;
                                }
                                else if (IsDigit(caracter.ToString()))
                                {
                                    state = 4;
                                    lexema = lexema + Char.ToString(caracter);
                                    break;
                                }
                                else
                                {
                                    return new Token(ERROR, "Simbolo Indefinido");
                                }
                        }
                        break;
                    case 1:
                        if (caracter == '=')
                        {
                            lexema = lexema + Char.ToString(caracter);
                            return new Token(OPERADORRELACIONAL, lexema);
                        }
                        else
                        {
                            Retract();
                            CONDICIONALES++;
                            return new Token(OPERADORRELACIONAL, lexema);
                        }
                    case 2:
                        if (caracter == '=')
                        {
                            CONDICIONALES++;
                            return new Token(OPERADORRELACIONAL, "==");
                        }
                        else
                        {
                            Retract();
                            ASIGNACIONES++;
                            return new Token(ASIGNACION, "=");
                        }
                    case 3:
                        if (caracter == '=')
                        {
                            CONDICIONALES++;
                            return new Token(OPERADORRELACIONAL, "!=");
                        }
                        else
                        {
                            Retract();
                            return new Token(ERROR, "Se esperaba '=' despues de '!'");
                        }
                    case 4:
                        if (!IsDigit(caracter.ToString()))
                        {
                            Retract();
                            return new Token(NUMERONATURAL, lexema);
                        }
                        lexema = lexema + Char.ToString(caracter);
                        break;
                    case 5:
                        if (IsLetter(caracter.ToString()) || IsDigit(caracter.ToString()) || caracter == '_')
                        {
                            lexema = lexema + Char.ToString(caracter);
                            break;
                        }
                        else
                        {
                            if (Array.Exists(palabrasReservadas, s => s.Equals(lexema)))
                            {
                                if (lexema.ToLower() == "for")
                                {
                                    LOOPS++;
                                }
                                else if (lexema.ToLower() == "const")
                                {
                                    CONSTANTES++;
                                }
                                Retract();
                                return new Token(PALABRARESERVADA, lexema);
                            }
                            if (Array.Exists(tiposDeDato, s => s.Equals(lexema)))
                            {
                                Retract();
                                VARIABLES++;
                                return new Token(TIPODEDATO, lexema);
                            }
                            else
                            {
                                Retract();
                                return new Token(IDENTIFICADOR, lexema);
                            }
                        }
                    case 6:
                        if (caracter == '/')
                        {
                            state = 7;
                            break;
                        }
                        else if (caracter == '*')
                        {
                            state = 8;
                            break;
                        }
                        else
                        {
                            Retract();
                            COMENTARIOS++;
                            return new Token(OPERADORARITMETICO, "/");
                        }
                    case 7:
                        if (caracter != '\n')
                        {
                            lexema = lexema + Char.ToString(caracter);
                            break;
                        }
                        if (caracter == '\n')
                        {
                            state = 0;
                            line++;
                            COMENTARIOS++;
                            return new Token(COMENTARIOMONOLINEA, lexema);
                        }
                        break;
                    case 8:
                        if (caracter != '*' && IsLetter(caracter.ToString()) || caracter == ' ')
                        {
                            lexema = lexema + Char.ToString(caracter);
                        }
                        if (caracter == '\n')
                        {
                            line++;
                        }
                        else if (caracter == '*')
                        {
                            state = 9;
                        }
                        break;
                    case 9:
                        if (caracter == '*')
                        {
                            break;
                        }
                        else if (caracter != '/')
                        {
                            if (IsLetter(caracter.ToString()) || caracter == ' ')
                            {
                                lexema = lexema + Char.ToString(caracter);
                            }
                            else if (caracter == '\n')
                            {
                                line++;
                                break;
                            }
                            state = 8;
                            break;
                        }
                        else
                        {
                            state = 0;
                            return new Token(COMENTARIOMULTILINEA, lexema);
                        }
                    default:
                        break;
                } // FIN DE SWITCH(STATE)
            } // FIN DE WHILE
        } // FIN DE SETLEXEMA

        public Automata()
        {
            filePath = IsUnix
            ? Path.Combine
                      (Directory.GetParent
                       (Directory.GetParent
                        (Directory.GetCurrentDirectory()).ToString()).Parent.FullName, "GitHub/Ember/Ember/Input/Input.txt")
                : Path.Combine
                      (Directory.GetParent
                       (Directory.GetParent
                        (Directory.GetCurrentDirectory()).ToString()).Parent.FullName, "Input\\Input.txt");

           try
            {
                input = File.OpenText(filePath);
            }
            catch (FileNotFoundException ex)
            {
                Console.Write(ex);
            }
            forward = 0;
            inicial = 0;
            line = 1;
        }

        public char NextChar()
        {
            if (inicial == 0)
            {
                string caracter;
                try
                {
                    while ((caracter = input.ReadLine()) != null)
                    {
                        buffer += caracter + '\n';
                    }
                    inicial = 1;
                    input.Close();
                    buffer = buffer + "~";
                }
                catch (FileNotFoundException fileNotFound)
                {
                    Console.WriteLine("Archivo no encontrado: " + fileNotFound);
                }
            }
            return buffer[forward++];
        }

        bool IsLetter(String input)
        {
            for (int i = 0; i != input.Length; ++i)
            {
                if (!Char.IsLetter(input.ElementAt(i)))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsDigit(string s)
        {
            foreach (char c in s)
            {
                if (!Char.IsDigit(c))
                    return false;
            }
            return s.Any();
        }

        public static bool IsUnix
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public void Retract()
        {
            forward = forward - 1;
        }

        public int GetLine()
        {
            return line;
        }

        public static int GetPARAB()
        {
            return PARENTESISABIERTO;
        }

        public static int GetPARCE()
        {
            return PARENTESISCERRADO;
        }

        public static int GetASIG()
        {
            return ASIGNACION;
        }

        public static int GetOPARIT()
        {
            return OPERADORARITMETICO;
        }

        public static int GetERR()
        {
            return ERROR;
        }

        public static int GetOPREL()
        {
            return OPERADORRELACIONAL;
        }

        public static int GetID()
        {
            return IDENTIFICADOR;
        }

        public static int GetNATURAL()
        {
            return NUMERONATURAL;
        }

        public static int GetEOS()
        {
            return ENDOFFILE;
        }

        public int GetForward()
        {
            return forward;
        }
    }
}
