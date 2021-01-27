# ESAPI_MoveStructures

This script helps you to Copy&amp;Move a structure in x, y and z-direction in a opened StructueSet. 

This script has no complex GUI but a simple way to accept UserInput (see 'public class DialogResult' and 'class SelectStructureWindow'). All code is in one file and should be easy to implement in your own scripts.

Workflow:

1.) Select the Base-Stucture from selectionWindow.
2.) Enter x-direction in mm, enter y-direction in mm and enter z-direction in slices (resulting z-displacement in mm depends on the CT slice thickness).
3.) The new sctructure has a automatic generated ID that is never to long and include the displacements (but rounded because only 13 chars are possible).

First-Compile tips:

add your own ESAPI-DLL-Files (VMS.TPS.Common.Model.API.dll + VMS.TPS.Common.Model.Types). Usually found in C:\Program Files\Varian\RTM\15.1\esapi\API
For clinical Mode: Approve the produced .dll in External Treatment Planning if 'Is Writeable = true'

Note:
- some of the code is based on a Reddit comment from FHesp (https://www.reddit.com/r/esapi/comments/i3e4c9/moving_a_structure_with_esapi/)
- absolute ESAPI-beginner should first look at my GettingStartedMaterial (collection of many helpful stuff from me or others and even includes a PDF version of some ESAPI-OnlineHelps)
https://drive.google.com/drive/folders/1-aYUOIfyvAUKtBg9TgEETiz4SYPonDOO
