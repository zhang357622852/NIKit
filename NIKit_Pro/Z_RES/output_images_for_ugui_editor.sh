# set params
SRC_FOLDER=${PWD}/Atlas
DEST_FOLDER=${PWD}/../Assets/Resources/Atlas


# output
# e.g (no auto-sd)
# TexturePacker --smart-update $file --data ${DEST_FOLDER}/${folderName}.plist --format cocos2d --sheet ${DEST_FOLDER}/${folderName}.png --enable-rotation
#
# e.g (auto-sd)
# TexturePacker --smart-update $file --data ${DEST_FOLDER}/${folderName}-hd.plist --format cocos2d --sheet ${DEST_FOLDER}/${folderName}-hd.png --enable-rotation --auto-sd
if [ ! $# == 2 ]; then

echo "Usage: $0 atlas_folder atlas_name"

exit

fi

if [ x$1 != x ]; then
#...有参数
    folderName=$1
    echo "$folderName Packer..."
    TexturePacker --smart-update $SRC_FOLDER/$folderName --data ${SRC_FOLDER}/${folderName}/$2.txt --format unity --sheet ${DEST_FOLDER}/${folderName}/$2.png --disable-rotation --no-trim --reduce-border-artifacts
fi
# else
# #...没有参数
#     for file in $SRC_FOLDER/*
#     do
#         if [ -d $file ]; then
#         folderName=${file##*/}
#         echo "$folderName Packer..."
#         TexturePacker --smart-update $file --data ${SRC_FOLDER}/${folderName}.txt --format unity --sheet ${DEST_FOLDER}/${folderName}.png --disable-rotation --no-trim --reduce-border-artifacts

#         fi
#     done
# fi
# if [ "$2" = "alpha" ]; then
#     cp ${DEST_FOLDER}/${folderName}.png ${DEST_FOLDER}/${folderName}_alpha.png
#     echo "copy ${DEST_FOLDER}/${folderName}.png to ${DEST_FOLDER}/${folderName}_alpha.png"
# fi
