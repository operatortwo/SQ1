Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Globalization
Imports System.Resources
Imports System.Windows

' Allgemeine Informationen über eine Assembly werden über die folgenden
' Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
' die einer Assembly zugeordnet sind.

' Werte der Assemblyattribute überprüfen

<Assembly: AssemblyTitle("SequencerUI")>
<Assembly: AssemblyDescription("User Interface for sequencer")>
<Assembly: AssemblyCompany("")>
<Assembly: AssemblyProduct("SequencerUI")>
<Assembly: AssemblyCopyright("Copyright @  2023   operatortwo")>
<Assembly: AssemblyTrademark("")> 
<Assembly: ComVisible(false)>

'Um mit dem Erstellen lokalisierbarer Anwendungen zu beginnen, legen Sie
'<UICulture>ImCodeVerwendeteKultur</UICulture> in der VBPROJ-Datei
'in einer <PropertyGroup> fest.  Wenn Sie in den Quelldateien beispielsweise Deutsch
'(Deutschland) verwenden, legen Sie <UICulture> auf "de-DE" fest.  Heben Sie dann die Auskommentierung
'des nachstehenden NeutralResourceLanguage-Attributs auf.  Aktualisieren Sie "en-US" in der nachstehenden Zeile,
'sodass es mit der UICulture-Einstellung in der Projektdatei übereinstimmt.

'<Assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)>


'Das ThemeInfo-Attribut beschreibt, wo Sie designspezifische und generische Ressourcenwörterbücher finden.
'1. Parameter: Speicherort der designspezifischen Ressourcenwörterbücher
'(wird verwendet, wenn eine Ressource auf der Seite nicht gefunden wird,
' oder in den Anwendungsressourcen-Wörterbüchern nicht gefunden werden kann.)

'2. Parameter: Speicherort des generischen Ressourcenwörterbuchs
'(wird verwendet, wenn eine Ressource auf der Seite nicht gefunden wird,
'in der Anwendung sowie in den designspezifischen Ressourcenwörterbüchern nicht gefunden werden kann)
<Assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)>



'Die folgende GUID wird für die typelib-ID verwendet, wenn dieses Projekt für COM verfügbar gemacht wird.
<Assembly: Guid("28abc02d-ba3b-4718-ad88-fb8515e0cb60")>

' Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
'
'      Hauptversion
'      Nebenversion
'      Buildnummer
'      Revision
'
' Sie können alle Werte angeben oder Standardwerte für die Build- und Revisionsnummern verwenden,
' indem Sie "*" wie unten gezeigt eingeben:
' <Assembly: AssemblyVersion("1.0.*")>

<Assembly: AssemblyVersion("1.0.2.2")>
<Assembly: AssemblyFileVersion("1.0.2.2")>
