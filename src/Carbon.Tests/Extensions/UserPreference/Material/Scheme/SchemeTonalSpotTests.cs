﻿using HizenLabs.Extensions.UserPreference.Material.ColorSpaces;
using HizenLabs.Extensions.UserPreference.Material.DynamicColors;
using HizenLabs.Extensions.UserPreference.Material.Scheme;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Carbon.Tests.Extensions.UserPreference.Material.Scheme;

[TestClass]
public class SchemeTonalSpotTests
{
    [TestMethod]
    [DataRow(0xFFFF0000, false, 0.0, 0xFF904B40, 0xFFFFFFFF, 0xFFFFDAD4, 0xFF73342A, 0xFF775651, 0xFFFFFFFF, 0xFFFFDAD4, 0xFF5D3F3B, 0xFF705C2E, 0xFFFFFFFF, 0xFFFBDFA6, 0xFF564419, 0xFFFFF8F6, 0xFF231918, 0xFFFFF8F6, 0xFF231918, 0xFFF5DDDA, 0xFF534341, 0xFF857370, 0xFF392E2C, 0xFFFFEDEA, 0xFFFFB4A8)]
    [DataRow(0xFFFF0000, false, 0.5, 0xFF5E241C, 0xFFFFFFFF, 0xFFA1594D, 0xFFFFFFFF, 0xFF4B2F2B, 0xFFFFFFFF, 0xFF87655F, 0xFFFFFFFF, 0xFF443409, 0xFFFFFFFF, 0xFF7F6A3B, 0xFFFFFFFF, 0xFFFFF8F6, 0xFF231918, 0xFFFFF8F6, 0xFF180F0E, 0xFFF5DDDA, 0xFF413330, 0xFF5F4F4C, 0xFF392E2C, 0xFFFFEDEA, 0xFFFFB4A8)]
    [DataRow(0xFFFF0000, false, 1.0, 0xFF511A13, 0xFFFFFFFF, 0xFF76362D, 0xFFFFFFFF, 0xFF3F2521, 0xFFFFFFFF, 0xFF60423D, 0xFFFFFFFF, 0xFF392A01, 0xFFFFFFFF, 0xFF59471B, 0xFFFFFFFF, 0xFFFFF8F6, 0xFF231918, 0xFFFFF8F6, 0xFF000000, 0xFFF5DDDA, 0xFF000000, 0xFF372927, 0xFF392E2C, 0xFFFFFFFF, 0xFFFFB4A8)]
    [DataRow(0xFFFF0000, true, 0.0, 0xFFFFB4A8, 0xFF561E16, 0xFF73342A, 0xFFFFDAD4, 0xFFE7BDB6, 0xFF442925, 0xFF5D3F3B, 0xFFFFDAD4, 0xFFDEC48C, 0xFF3E2E04, 0xFF564419, 0xFFFBDFA6, 0xFF1A1110, 0xFFF1DFDC, 0xFF1A1110, 0xFFF1DFDC, 0xFF534341, 0xFFD8C2BE, 0xFFA08C89, 0xFFF1DFDC, 0xFF392E2C, 0xFF904B40)]
    [DataRow(0xFFFF0000, true, 0.5, 0xFFFFD2CB, 0xFF48140D, 0xFFCC7B6F, 0xFF000000, 0xFFFED2CB, 0xFF381F1B, 0xFFAE8882, 0xFF000000, 0xFFF5D9A0, 0xFF322300, 0xFFA58E5B, 0xFF000000, 0xFF1A1110, 0xFFF1DFDC, 0xFF1A1110, 0xFFFFFFFF, 0xFF534341, 0xFFEED7D3, 0xFFC2ADAA, 0xFFF1DFDC, 0xFF322826, 0xFF74352C)]
    [DataRow(0xFFFF0000, true, 1.0, 0xFFFFECE9, 0xFF000000, 0xFFFFAEA1, 0xFF220000, 0xFFFFECE9, 0xFF000000, 0xFFE3B9B2, 0xFF190604, 0xFFFFEED0, 0xFF000000, 0xFFDAC089, 0xFF110A00, 0xFF1A1110, 0xFFF1DFDC, 0xFF1A1110, 0xFFFFFFFF, 0xFF534341, 0xFFFFFFFF, 0xFFFFECE9, 0xFFF1DFDC, 0xFF000000, 0xFF74352C)]
    [DataRow(0xFF00FF00, false, 0.0, 0xFF406836, 0xFFFFFFFF, 0xFFC0EFB0, 0xFF285020, 0xFF54634D, 0xFFFFFFFF, 0xFFD7E8CD, 0xFF3C4B37, 0xFF386568, 0xFFFFFFFF, 0xFFBCEBEE, 0xFF1E4D50, 0xFFF8FBF1, 0xFF191D17, 0xFFF8FBF1, 0xFF191D17, 0xFFDFE4D7, 0xFF43483F, 0xFF73796E, 0xFF2E322B, 0xFFEFF2E8, 0xFFA5D395)]
    [DataRow(0xFF00FF00, false, 0.5, 0xFF173E11, 0xFFFFFFFF, 0xFF4E7743, 0xFFFFFFFF, 0xFF2C3A27, 0xFFFFFFFF, 0xFF62715B, 0xFFFFFFFF, 0xFF073D3F, 0xFFFFFFFF, 0xFF477477, 0xFFFFFFFF, 0xFFF8FBF1, 0xFF191D17, 0xFFF8FBF1, 0xFF0E120D, 0xFFDFE4D7, 0xFF32382F, 0xFF4E544A, 0xFF2E322B, 0xFFEFF2E8, 0xFFA5D395)]
    [DataRow(0xFF00FF00, false, 1.0, 0xFF0C3407, 0xFFFFFFFF, 0xFF2B5222, 0xFFFFFFFF, 0xFF22301E, 0xFFFFFFFF, 0xFF3F4D39, 0xFFFFFFFF, 0xFF003234, 0xFFFFFFFF, 0xFF215053, 0xFFFFFFFF, 0xFFF8FBF1, 0xFF191D17, 0xFFF8FBF1, 0xFF000000, 0xFFDFE4D7, 0xFF000000, 0xFF282E25, 0xFF2E322B, 0xFFFFFFFF, 0xFFA5D395)]
    [DataRow(0xFF00FF00, true, 0.0, 0xFFA5D395, 0xFF11380B, 0xFF285020, 0xFFC0EFB0, 0xFFBBCBB2, 0xFF263422, 0xFF3C4B37, 0xFFD7E8CD, 0xFFA0CFD2, 0xFF003739, 0xFF1E4D50, 0xFFBCEBEE, 0xFF11140F, 0xFFE1E4DA, 0xFF11140F, 0xFFE1E4DA, 0xFF43483F, 0xFFC3C8BC, 0xFF8D9387, 0xFFE1E4DA, 0xFF2E322B, 0xFF406836)]
    [DataRow(0xFF00FF00, true, 0.5, 0xFFBAE9AA, 0xFF052D03, 0xFF719C64, 0xFF000000, 0xFFD1E1C7, 0xFF1C2918, 0xFF86957E, 0xFF000000, 0xFFB6E5E8, 0xFF002B2D, 0xFF6B989B, 0xFF000000, 0xFF11140F, 0xFFE1E4DA, 0xFF11140F, 0xFFFFFFFF, 0xFF43483F, 0xFFD9DED1, 0xFFAEB4A8, 0xFFE1E4DA, 0xFF272B25, 0xFF2A5121)]
    [DataRow(0xFF00FF00, true, 1.0, 0xFFCEFDBC, 0xFF000000, 0xFFA1CF92, 0xFF000F00, 0xFFE4F5DA, 0xFF000000, 0xFFB7C8AE, 0xFF030E02, 0xFFC9F9FC, 0xFF000000, 0xFF9CCBCE, 0xFF000E0F, 0xFF11140F, 0xFFE1E4DA, 0xFF11140F, 0xFFFFFFFF, 0xFF43483F, 0xFFFFFFFF, 0xFFECF2E5, 0xFFE1E4DA, 0xFF000000, 0xFF2A5121)]
    [DataRow(0xFF0000FF, false, 0.0, 0xFF555992, 0xFFFFFFFF, 0xFFE0E0FF, 0xFF3E4278, 0xFF5C5D72, 0xFFFFFFFF, 0xFFE1E0F9, 0xFF444559, 0xFF78536B, 0xFFFFFFFF, 0xFFFFD8EE, 0xFF5E3C52, 0xFFFBF8FF, 0xFF1B1B21, 0xFFFBF8FF, 0xFF1B1B21, 0xFFE4E1EC, 0xFF46464F, 0xFF777680, 0xFF303036, 0xFFF2EFF7, 0xFFBEC2FF)]
    [DataRow(0xFF0000FF, false, 0.5, 0xFF2D3167, 0xFFFFFFFF, 0xFF6468A2, 0xFFFFFFFF, 0xFF343548, 0xFFFFFFFF, 0xFF6B6B81, 0xFFFFFFFF, 0xFF4C2C41, 0xFFFFFFFF, 0xFF88617A, 0xFFFFFFFF, 0xFFFBF8FF, 0xFF1B1B21, 0xFFFBF8FF, 0xFF111116, 0xFFE4E1EC, 0xFF36353E, 0xFF52525B, 0xFF303036, 0xFFF2EFF7, 0xFFBEC2FF)]
    [DataRow(0xFF0000FF, false, 1.0, 0xFF22265C, 0xFFFFFFFF, 0xFF40447B, 0xFFFFFFFF, 0xFF2A2B3D, 0xFFFFFFFF, 0xFF47485C, 0xFFFFFFFF, 0xFF412237, 0xFFFFFFFF, 0xFF613E55, 0xFFFFFFFF, 0xFFFBF8FF, 0xFF1B1B21, 0xFFFBF8FF, 0xFF000000, 0xFFE4E1EC, 0xFF000000, 0xFF2B2B34, 0xFF303036, 0xFFFFFFFF, 0xFFBEC2FF)]
    [DataRow(0xFF0000FF, true, 0.0, 0xFFBEC2FF, 0xFF272B60, 0xFF3E4278, 0xFFE0E0FF, 0xFFC5C4DD, 0xFF2E2F42, 0xFF444559, 0xFFE1E0F9, 0xFFE8B9D5, 0xFF46263B, 0xFF5E3C52, 0xFFFFD8EE, 0xFF131318, 0xFFE4E1E9, 0xFF131318, 0xFFE4E1E9, 0xFF46464F, 0xFFC7C5D0, 0xFF91909A, 0xFFE4E1E9, 0xFF303036, 0xFF555992)]
    [DataRow(0xFF0000FF, true, 0.5, 0xFFD9D9FF, 0xFF1C1F55, 0xFF888CC8, 0xFF000000, 0xFFDBDAF3, 0xFF232436, 0xFF8F8FA6, 0xFF000000, 0xFFFFCFEB, 0xFF391B30, 0xFFAE849E, 0xFF000000, 0xFF131318, 0xFFE4E1E9, 0xFF131318, 0xFFFFFFFF, 0xFF46464F, 0xFFDDDBE6, 0xFFB2B1BB, 0xFFE4E1E9, 0xFF2A292F, 0xFF3F437A)]
    [DataRow(0xFF0000FF, true, 1.0, 0xFFF0EEFF, 0xFF000000, 0xFFBABEFD, 0xFF00003C, 0xFFF0EEFF, 0xFF000000, 0xFFC1C0D9, 0xFF080A1B, 0xFFFFEBF4, 0xFF000000, 0xFFE4B5D1, 0xFF1B0315, 0xFF131318, 0xFFE4E1E9, 0xFF131318, 0xFFFFFFFF, 0xFF46464F, 0xFFFFFFFF, 0xFFF1EEFA, 0xFFE4E1E9, 0xFF000000, 0xFF3F437A)]
    [DataRow(0xFFFFA500, false, 0.0, 0xFF815512, 0xFFFFFFFF, 0xFFFFDDB7, 0xFF653E00, 0xFF705B41, 0xFFFFFFFF, 0xFFFCDEBC, 0xFF57432B, 0xFF53643E, 0xFFFFFFFF, 0xFFD6E9B9, 0xFF3C4C28, 0xFFFFF8F4, 0xFF211A13, 0xFFFFF8F4, 0xFF211A13, 0xFFF0E0D0, 0xFF504539, 0xFF827568, 0xFF362F27, 0xFFFCEEE2, 0xFFF7BB70)]
    [DataRow(0xFFFFA500, false, 0.5, 0xFF4E2F00, 0xFFFFFFFF, 0xFF926321, 0xFFFFFFFF, 0xFF45331C, 0xFFFFFFFF, 0xFF80694E, 0xFFFFFFFF, 0xFF2C3B19, 0xFFFFFFFF, 0xFF62734B, 0xFFFFFFFF, 0xFFFFF8F4, 0xFF211A13, 0xFFFFF8F4, 0xFF161009, 0xFFF0E0D0, 0xFF3E342A, 0xFF5C5144, 0xFF362F27, 0xFFFCEEE2, 0xFFF7BB70)]
    [DataRow(0xFFFFA500, false, 1.0, 0xFF412600, 0xFFFFFFFF, 0xFF684100, 0xFFFFFFFF, 0xFF3A2913, 0xFFFFFFFF, 0xFF5A462D, 0xFFFFFFFF, 0xFF223010, 0xFFFFFFFF, 0xFF3F4E2A, 0xFFFFFFFF, 0xFFFFF8F4, 0xFF211A13, 0xFFFFF8F4, 0xFF000000, 0xFFF0E0D0, 0xFF000000, 0xFF342B20, 0xFF362F27, 0xFFFFFFFF, 0xFFF7BB70)]
    [DataRow(0xFFFFA500, true, 0.0, 0xFFF7BB70, 0xFF462A00, 0xFF653E00, 0xFFFFDDB7, 0xFFDFC2A2, 0xFF3F2D17, 0xFF57432B, 0xFFFCDEBC, 0xFFBACD9F, 0xFF263514, 0xFF3C4C28, 0xFFD6E9B9, 0xFF18120C, 0xFFEEE0D4, 0xFF18120C, 0xFFEEE0D4, 0xFF504539, 0xFFD4C4B5, 0xFF9C8E80, 0xFFEEE0D4, 0xFF362F27, 0xFF815512)]
    [DataRow(0xFFFFA500, true, 0.5, 0xFFFFD5A5, 0xFF382100, 0xFFBB8641, 0xFF000000, 0xFFF5D7B7, 0xFF33220D, 0xFFA68C6F, 0xFF000000, 0xFFD0E3B3, 0xFF1C2A0A, 0xFF85976C, 0xFF000000, 0xFF18120C, 0xFFEEE0D4, 0xFF18120C, 0xFFFFFFFF, 0xFF504539, 0xFFEADACA, 0xFFBEAFA1, 0xFFEEE0D4, 0xFF302921, 0xFF663F00)]
    [DataRow(0xFFFFA500, true, 1.0, 0xFFFFEDDC, 0xFF000000, 0xFFF3B86D, 0xFF140900, 0xFFFFEDDC, 0xFF000000, 0xFFDABE9E, 0xFF140900, 0xFFE4F7C6, 0xFF000000, 0xFFB7C99B, 0xFF050E00, 0xFF18120C, 0xFFEEE0D4, 0xFF18120C, 0xFFFFFFFF, 0xFF504539, 0xFFFFFFFF, 0xFFFEEDDD, 0xFFEEE0D4, 0xFF000000, 0xFF663F00)]
    [DataRow(0xFF800080, false, 0.0, 0xFF804D7A, 0xFFFFFFFF, 0xFFFFD7F5, 0xFF653661, 0xFF6D5869, 0xFFFFFFFF, 0xFFF7DAEF, 0xFF554151, 0xFF825345, 0xFFFFFFFF, 0xFFFFDBD1, 0xFF663C2F, 0xFFFFF7F9, 0xFF201A1E, 0xFFFFF7F9, 0xFF201A1E, 0xFFEEDEE7, 0xFF4E444B, 0xFF80747C, 0xFF352E33, 0xFFFAEDF4, 0xFFF1B3E6)]
    [DataRow(0xFF800080, false, 0.5, 0xFF52254F, 0xFFFFFFFF, 0xFF905B89, 0xFFFFFFFF, 0xFF433040, 0xFFFFFFFF, 0xFF7D6678, 0xFFFFFFFF, 0xFF532B20, 0xFFFFFFFF, 0xFF926153, 0xFFFFFFFF, 0xFFFFF7F9, 0xFF201A1E, 0xFFFFF7F9, 0xFF150F14, 0xFFEEDEE7, 0xFF3D333A, 0xFF5A4F57, 0xFF352E33, 0xFFFAEDF4, 0xFFF1B3E6)]
    [DataRow(0xFF800080, false, 1.0, 0xFF471B44, 0xFFFFFFFF, 0xFF683863, 0xFFFFFFFF, 0xFF382635, 0xFFFFFFFF, 0xFF574353, 0xFFFFFFFF, 0xFF472217, 0xFFFFFFFF, 0xFF693E31, 0xFFFFFFFF, 0xFFFFF7F9, 0xFF201A1E, 0xFFFFF7F9, 0xFF000000, 0xFFEEDEE7, 0xFF000000, 0xFF322930, 0xFF352E33, 0xFFFFFFFF, 0xFFF1B3E6)]
    [DataRow(0xFF800080, true, 0.0, 0xFFF1B3E6, 0xFF4C1F49, 0xFF653661, 0xFFFFD7F5, 0xFFDABFD2, 0xFF3D2B3A, 0xFF554151, 0xFFF7DAEF, 0xFFF5B8A7, 0xFF4C261B, 0xFF663C2F, 0xFFFFDBD1, 0xFF171216, 0xFFECDFE5, 0xFF171216, 0xFFECDFE5, 0xFF4E444B, 0xFFD1C2CB, 0xFF9A8D95, 0xFFECDFE5, 0xFF352E33, 0xFF804D7A)]
    [DataRow(0xFF800080, true, 0.5, 0xFFFFCDF4, 0xFF3F143D, 0xFFB77EAE, 0xFF000000, 0xFFF1D4E8, 0xFF31202F, 0xFFA2899C, 0xFF000000, 0xFFFFD2C6, 0xFF3F1B11, 0xFFBA8474, 0xFF000000, 0xFF171216, 0xFFECDFE5, 0xFF171216, 0xFFFFFFFF, 0xFF4E444B, 0xFFE7D8E1, 0xFFBCAEB7, 0xFFECDFE5, 0xFF2F282D, 0xFF673762)]
    [DataRow(0xFF800080, true, 1.0, 0xFFFFEAF7, 0xFF000000, 0xFFEDAFE2, 0xFF1C001C, 0xFFFFEAF7, 0xFF000000, 0xFFD6BBCE, 0xFF150613, 0xFFFFECE7, 0xFF000000, 0xFFF1B4A3, 0xFF1D0300, 0xFF171216, 0xFFECDFE5, 0xFF171216, 0xFFFFFFFF, 0xFF4E444B, 0xFFFFFFFF, 0xFFFCECF5, 0xFFECDFE5, 0xFF000000, 0xFF673762)]
    [DataRow(0xFF00CED1, false, 0.0, 0xFF00696B, 0xFFFFFFFF, 0xFF9CF1F2, 0xFF004F51, 0xFF4A6363, 0xFFFFFFFF, 0xFFCCE8E8, 0xFF324B4B, 0xFF4C5F7C, 0xFFFFFFFF, 0xFFD4E3FF, 0xFF344863, 0xFFF4FBFA, 0xFF161D1D, 0xFFF4FBFA, 0xFF161D1D, 0xFFDAE4E4, 0xFF3F4949, 0xFF6F7979, 0xFF2B3232, 0xFFECF2F1, 0xFF80D4D6)]
    [DataRow(0xFF00CED1, false, 0.5, 0xFF003D3E, 0xFFFFFFFF, 0xFF16797B, 0xFFFFFFFF, 0xFF213A3B, 0xFFFFFFFF, 0xFF587272, 0xFFFFFFFF, 0xFF233752, 0xFFFFFFFF, 0xFF5B6E8C, 0xFFFFFFFF, 0xFFF4FBFA, 0xFF161D1D, 0xFFF4FBFA, 0xFF0C1212, 0xFFDAE4E4, 0xFF2E3838, 0xFF4A5454, 0xFF2B3232, 0xFFECF2F1, 0xFF80D4D6)]
    [DataRow(0xFF00CED1, false, 1.0, 0xFF003233, 0xFFFFFFFF, 0xFF005253, 0xFFFFFFFF, 0xFF173031, 0xFFFFFFFF, 0xFF344E4E, 0xFFFFFFFF, 0xFF192D47, 0xFFFFFFFF, 0xFF374A66, 0xFFFFFFFF, 0xFFF4FBFA, 0xFF161D1D, 0xFFF4FBFA, 0xFF000000, 0xFFDAE4E4, 0xFF000000, 0xFF242E2E, 0xFF2B3232, 0xFFFFFFFF, 0xFF80D4D6)]
    [DataRow(0xFF00CED1, true, 0.0, 0xFF80D4D6, 0xFF003738, 0xFF004F51, 0xFF9CF1F2, 0xFFB0CCCC, 0xFF1B3435, 0xFF324B4B, 0xFFCCE8E8, 0xFFB4C8E9, 0xFF1D314C, 0xFF344863, 0xFFD4E3FF, 0xFF0E1415, 0xFFDDE4E3, 0xFF0E1415, 0xFFDDE4E3, 0xFF3F4949, 0xFFBEC8C8, 0xFF899392, 0xFFDDE4E3, 0xFF2B3232, 0xFF00696B)]
    [DataRow(0xFF00CED1, true, 0.5, 0xFF96EBEC, 0xFF002B2C, 0xFF479E9F, 0xFF000000, 0xFFC6E2E2, 0xFF102A2A, 0xFF7B9696, 0xFF000000, 0xFFCADDFF, 0xFF112640, 0xFF7E92B1, 0xFF000000, 0xFF0E1415, 0xFFDDE4E3, 0xFF0E1415, 0xFFFFFFFF, 0xFF3F4949, 0xFFD4DEDE, 0xFFAAB4B4, 0xFFDDE4E3, 0xFF252B2B, 0xFF005152)]
    [DataRow(0xFF00CED1, true, 1.0, 0xFFAEFEFF, 0xFF000000, 0xFF7CD0D2, 0xFF000E0E, 0xFFD9F5F5, 0xFF000000, 0xFFADC8C8, 0xFF000E0E, 0xFFEAF0FF, 0xFF000000, 0xFFB0C4E5, 0xFF000B1D, 0xFF0E1415, 0xFFDDE4E3, 0xFF0E1415, 0xFFFFFFFF, 0xFF3F4949, 0xFFFFFFFF, 0xFFE8F2F1, 0xFFDDE4E3, 0xFF000000, 0xFF005152)]
    [DataRow(0xFFB22222, false, 0.0, 0xFF904A44, 0xFFFFFFFF, 0xFFFFDAD6, 0xFF73332E, 0xFF775653, 0xFFFFFFFF, 0xFFFFDAD6, 0xFF5D3F3C, 0xFF725B2E, 0xFFFFFFFF, 0xFFFEDFA6, 0xFF584419, 0xFFFFF8F7, 0xFF231918, 0xFFFFF8F7, 0xFF231918, 0xFFF5DDDB, 0xFF534341, 0xFF857371, 0xFF392E2D, 0xFFFFEDEA, 0xFFFFB4AC)]
    [DataRow(0xFFB22222, false, 0.5, 0xFF5E231F, 0xFFFFFFFF, 0xFFA15852, 0xFFFFFFFF, 0xFF4B2F2C, 0xFFFFFFFF, 0xFF876561, 0xFFFFFFFF, 0xFF463309, 0xFFFFFFFF, 0xFF816A3B, 0xFFFFFFFF, 0xFFFFF8F7, 0xFF231918, 0xFFFFF8F7, 0xFF180F0E, 0xFFF5DDDB, 0xFF413331, 0xFF5F4F4D, 0xFF392E2D, 0xFFFFEDEA, 0xFFFFB4AC)]
    [DataRow(0xFFB22222, false, 1.0, 0xFF511A16, 0xFFFFFFFF, 0xFF763630, 0xFFFFFFFF, 0xFF3F2523, 0xFFFFFFFF, 0xFF60423E, 0xFFFFFFFF, 0xFF3B2902, 0xFFFFFFFF, 0xFF5B461B, 0xFFFFFFFF, 0xFFFFF8F7, 0xFF231918, 0xFFFFF8F7, 0xFF000000, 0xFFF5DDDB, 0xFF000000, 0xFF362927, 0xFF392E2D, 0xFFFFFFFF, 0xFFFFB4AC)]
    [DataRow(0xFFB22222, true, 0.0, 0xFFFFB4AC, 0xFF561E1A, 0xFF73332E, 0xFFFFDAD6, 0xFFE7BDB8, 0xFF442927, 0xFF5D3F3C, 0xFFFFDAD6, 0xFFE0C38C, 0xFF3F2D04, 0xFF584419, 0xFFFEDFA6, 0xFF1A1110, 0xFFF1DEDC, 0xFF1A1110, 0xFFF1DEDC, 0xFF534341, 0xFFD8C2BF, 0xFFA08C8A, 0xFFF1DEDC, 0xFF392E2D, 0xFF904A44)]
    [DataRow(0xFFB22222, true, 0.5, 0xFFFFD2CD, 0xFF481310, 0xFFCC7B73, 0xFF000000, 0xFFFED2CD, 0xFF381F1C, 0xFFAD8884, 0xFF000000, 0xFFF7D8A0, 0xFF332300, 0xFFA78D5B, 0xFF000000, 0xFF1A1110, 0xFFF1DEDC, 0xFF1A1110, 0xFFFFFFFF, 0xFF534341, 0xFFEED7D5, 0xFFC2ADAB, 0xFFF1DEDC, 0xFF322826, 0xFF74352F)]
    [DataRow(0xFFB22222, true, 1.0, 0xFFFFECE9, 0xFF000000, 0xFFFFAEA6, 0xFF220001, 0xFFFFECE9, 0xFF000000, 0xFFE3B9B4, 0xFF190605, 0xFFFFEED3, 0xFF000000, 0xFFDCBF89, 0xFF120A00, 0xFF1A1110, 0xFFF1DEDC, 0xFF1A1110, 0xFFFFFFFF, 0xFF534341, 0xFFFFFFFF, 0xFFFFECE9, 0xFFF1DEDC, 0xFF000000, 0xFF74352F)]
    [DataRow(0xFF1E90FF, false, 0.0, 0xFF3C6090, 0xFFFFFFFF, 0xFFD4E3FF, 0xFF224876, 0xFF545F71, 0xFFFFFFFF, 0xFFD8E3F8, 0xFF3D4758, 0xFF6E5676, 0xFFFFFFFF, 0xFFF7D8FF, 0xFF553F5D, 0xFFF9F9FF, 0xFF191C20, 0xFFF9F9FF, 0xFF191C20, 0xFFE0E2EC, 0xFF43474E, 0xFF74777F, 0xFF2E3035, 0xFFF0F0F7, 0xFFA6C8FF)]
    [DataRow(0xFF1E90FF, false, 0.5, 0xFF0B3765, 0xFFFFFFFF, 0xFF4C6E9F, 0xFFFFFFFF, 0xFF2C3747, 0xFFFFFFFF, 0xFF636E80, 0xFFFFFFFF, 0xFF432E4C, 0xFFFFFFFF, 0xFF7D6485, 0xFFFFFFFF, 0xFFF9F9FF, 0xFF191C20, 0xFFF9F9FF, 0xFF0F1116, 0xFFE0E2EC, 0xFF33363D, 0xFF4F525A, 0xFF2E3035, 0xFFF0F0F7, 0xFFA6C8FF)]
    [DataRow(0xFF1E90FF, false, 1.0, 0xFF002C57, 0xFFFFFFFF, 0xFF254A79, 0xFFFFFFFF, 0xFF222D3C, 0xFFFFFFFF, 0xFF3F4A5B, 0xFFFFFFFF, 0xFF392441, 0xFFFFFFFF, 0xFF574160, 0xFFFFFFFF, 0xFFF9F9FF, 0xFF191C20, 0xFFF9F9FF, 0xFF000000, 0xFFE0E2EC, 0xFF000000, 0xFF282C33, 0xFF2E3035, 0xFFFFFFFF, 0xFFA6C8FF)]
    [DataRow(0xFF1E90FF, true, 0.0, 0xFFA6C8FF, 0xFF01315E, 0xFF224876, 0xFFD4E3FF, 0xFFBCC7DC, 0xFF273141, 0xFF3D4758, 0xFFD8E3F8, 0xFFDABDE2, 0xFF3D2946, 0xFF553F5D, 0xFFF7D8FF, 0xFF111318, 0xFFE1E2E9, 0xFF111318, 0xFFE1E2E9, 0xFF43474E, 0xFFC3C6CF, 0xFF8D9199, 0xFFE1E2E9, 0xFF2E3035, 0xFF3C6090)]
    [DataRow(0xFF1E90FF, true, 0.5, 0xFFCADDFF, 0xFF00264C, 0xFF7092C6, 0xFF000000, 0xFFD2DDF2, 0xFF1C2636, 0xFF8791A5, 0xFF000000, 0xFFF1D2F9, 0xFF321E3A, 0xFFA288AB, 0xFF000000, 0xFF111318, 0xFFE1E2E9, 0xFF111318, 0xFFFFFFFF, 0xFF43474E, 0xFFD9DCE5, 0xFFAFB2BB, 0xFFE1E2E9, 0xFF282A2F, 0xFF234978)]
    [DataRow(0xFF1E90FF, true, 1.0, 0xFFEAF0FF, 0xFF000000, 0xFFA2C4FB, 0xFF000B1E, 0xFFEAF0FF, 0xFF000000, 0xFFB9C3D8, 0xFF020B1A, 0xFFFDEAFF, 0xFF000000, 0xFFD6B9DE, 0xFF15041E, 0xFF111318, 0xFFE1E2E9, 0xFF111318, 0xFFFFFFFF, 0xFF43474E, 0xFFFFFFFF, 0xFFEDF0F9, 0xFFE1E2E9, 0xFF000000, 0xFF234978)]
    public void SchemeTonalSpot_Create_ReturnsExpectedScheme(
        uint argb,
        bool isDark,
        double contrastLevel,

        uint expectedPrimary,
        uint expectedOnPrimary,
        uint expectedPrimaryContainer,
        uint expectedOnPrimaryContainer,

        uint expectedSecondary,
        uint expectedOnSecondary,
        uint expectedSecondaryContainer,
        uint expectedOnSecondaryContainer,

        uint expectedTertiary,
        uint expectedOnTertiary,
        uint expectedTertiaryContainer,
        uint expectedOnTertiaryContainer,

        uint expectedBackground,
        uint expectedOnBackground,
        uint expectedSurface,
        uint expectedOnSurface,
        uint expectedSurfaceVariant,
        uint expectedOnSurfaceVariant,
        uint expectedOutline,

        uint expectedInverseSurface,
        uint expectedInverseOnSurface,
        uint expectedInversePrimary)
    {
        // Arrange
        StandardRgb color = argb;
        using var hct = Hct.Create(color);

        // Act
        var scheme = SchemeTonalSpot.Create(hct, isDark, contrastLevel);
        var spec = ColorSpecs.Get(scheme.SpecVersion);

        var actualPrimary = spec.Primary.GetColor(scheme);
        var actualOnPrimary = spec.OnPrimary.GetColor(scheme);
        var actualPrimaryContainer = spec.PrimaryContainer.GetColor(scheme);
        var actualOnPrimaryContainer = spec.OnPrimaryContainer.GetColor(scheme);

        var actualSecondary = spec.Secondary.GetColor(scheme);
        var actualOnSecondary = spec.OnSecondary.GetColor(scheme);
        var actualSecondaryContainer = spec.SecondaryContainer.GetColor(scheme);
        var actualOnSecondaryContainer = spec.OnSecondaryContainer.GetColor(scheme);

        var actualTertiary = spec.Tertiary.GetColor(scheme);
        var actualOnTertiary = spec.OnTertiary.GetColor(scheme);
        var actualTertiaryContainer = spec.TertiaryContainer.GetColor(scheme);
        var actualOnTertiaryContainer = spec.OnTertiaryContainer.GetColor(scheme);

        var actualBackground = spec.Background.GetColor(scheme);
        var actualOnBackground = spec.OnBackground.GetColor(scheme);
        var actualSurface = spec.Surface.GetColor(scheme);
        var actualOnSurface = spec.OnSurface.GetColor(scheme);
        var actualSurfaceVariant = spec.SurfaceVariant.GetColor(scheme);
        var actualOnSurfaceVariant = spec.OnSurfaceVariant.GetColor(scheme);
        var actualOutline = spec.Outline.GetColor(scheme);

        var actualInverseSurface = spec.InverseSurface.GetColor(scheme);
        var actualInverseOnSurface = spec.InverseOnSurface.GetColor(scheme);
        var actualInversePrimary = spec.InversePrimary.GetColor(scheme);

        // Assert
        Assert.IsNotNull(scheme);
        Assert.AreEqual(isDark, scheme.IsDark);
        Assert.AreEqual(contrastLevel, scheme.ContrastLevel, 0.01);

        AssertColorEqual(expectedPrimary, actualPrimary, "Primary", color);
        AssertColorEqual(expectedOnPrimary, actualOnPrimary, "OnPrimary", color);
        AssertColorEqual(expectedPrimaryContainer, actualPrimaryContainer, "PrimaryContainer", color);
        AssertColorEqual(expectedOnPrimaryContainer, actualOnPrimaryContainer, "OnPrimaryContainer", color);

        AssertColorEqual(expectedSecondary, actualSecondary, "Secondary", color);
        AssertColorEqual(expectedOnSecondary, actualOnSecondary, "OnSecondary", color);
        AssertColorEqual(expectedSecondaryContainer, actualSecondaryContainer, "SecondaryContainer", color);
        AssertColorEqual(expectedOnSecondaryContainer, actualOnSecondaryContainer, "OnSecondaryContainer", color);

        AssertColorEqual(expectedTertiary, actualTertiary, "Tertiary", color);
        AssertColorEqual(expectedOnTertiary, actualOnTertiary, "OnTertiary", color);
        AssertColorEqual(expectedTertiaryContainer, actualTertiaryContainer, "TertiaryContainer", color);
        AssertColorEqual(expectedOnTertiaryContainer, actualOnTertiaryContainer, "OnTertiaryContainer", color);

        AssertColorEqual(expectedBackground, actualBackground, "Background", color);
        AssertColorEqual(expectedOnBackground, actualOnBackground, "OnBackground", color);
        AssertColorEqual(expectedSurface, actualSurface, "Surface", color);
        AssertColorEqual(expectedOnSurface, actualOnSurface, "OnSurface", color);
        AssertColorEqual(expectedSurfaceVariant, actualSurfaceVariant, "SurfaceVariant", color);
        AssertColorEqual(expectedOnSurfaceVariant, actualOnSurfaceVariant, "OnSurfaceVariant", color);
        AssertColorEqual(expectedOutline, actualOutline, "Outline", color);

        AssertColorEqual(expectedInverseSurface, actualInverseSurface, "InverseSurface", color);
        AssertColorEqual(expectedInverseOnSurface, actualInverseOnSurface, "InverseOnSurface", color);
        AssertColorEqual(expectedInversePrimary, actualInversePrimary, "InversePrimary", color);
    }

    private static void AssertColorEqual(StandardRgb expected, StandardRgb actual, string roleName, StandardRgb color)
    {
        // Some of the reds end up being 1 off, which is acceptable.
        const int channelTolerance = 1;

        int deltaR = Math.Abs(expected.R - actual.R);
        int deltaG = Math.Abs(expected.G - actual.G);
        int deltaB = Math.Abs(expected.B - actual.B);

        int maxDiff = Math.Max(deltaR, Math.Max(deltaG, deltaB));
        double euclidean = Math.Sqrt(deltaR * deltaR + deltaG * deltaG + deltaB * deltaB);

        if (maxDiff <= channelTolerance)
            return;

        string message =
            $"{roleName} mismatch for color {color}.\n" +
            $"Expected: {expected}, Actual: {actual}\n" +
            $"ΔR: {deltaR}, ΔG: {deltaG}, ΔB: {deltaB}, MaxΔ: {maxDiff}, EuclideanΔ: {euclidean:F2}";

        Assert.Fail(message);
    }

}