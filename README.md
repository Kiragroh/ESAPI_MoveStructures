# ESAPI_MoveStructures
Copy&amp;Moving a structure with simple User-Input-Method

This script helps you to Copy&amp;Move a structure in x, y and z-direction in a opened StructueSet. 

This script has no complex GUI but a simple way to accept UserInput (see 'public class DialogResult' and 'class SelectStructureWindow'). All code is in one file and should be easy to implement in your own scripts.

Instructions:
1.) Compile the script with your own Esapi-Dependencies.
2.) Run the script (ScriptApproval is requiered in clinical mode)
3.) Selct the Base-Stucture.
4.) Enter x-direction in mm, enter y-direction in mm and enter z-direction in slices (resulting z-displacement in mm depends on the CT slice thickness).
