# Copyright by Genouka 2025
# Notice: The script is licensed under MPL2.0 so that you can use the script in other projects.
find ../UndertaleModToolAvalonia/Assets/sth/Scripts -type f \( -name "*.cs" -o -name "*.csx" \) -printf '%P;;' \
| sed 's/;;$//' \
> ../UndertaleModToolAvalonia/Assets/sth/Scripts/scriptpath.txt