# set params
SRC_FOLDER=${PWD}/Atlas
DEST_FOLDER=${PWD}/../Assets/GUI/AtlasRes


# output
# e.g (no auto-sd)
# TexturePacker --smart-update $file --data ${DEST_FOLDER}/${folderName}.plist --format cocos2d --sheet ${DEST_FOLDER}/${folderName}.png --enable-rotation
#
# e.g (auto-sd)
# TexturePacker --smart-update $file --data ${DEST_FOLDER}/${folderName}-hd.plist --format cocos2d --sheet ${DEST_FOLDER}/${folderName}-hd.png --enable-rotation --auto-sd

if [ x$1 != x ]; then
#...有参数
    folderName=$1
    echo "$folderName Packer..."
    TexturePacker --smart-update $SRC_FOLDER/$folderName --data ${DEST_FOLDER}/$2/$2.txt --format unity --sheet ${DEST_FOLDER}/$2/$2.png --disable-rotation --no-trim --reduce-border-artifacts
# else
# #...没有参数
#     for file in $SRC_FOLDER/*
#     do
#         if [ -d $file ]; then
#         folderName=${file##*/}
#         echo "$folderName Packer..."
#         TexturePacker --smart-update $file --data ${DEST_FOLDER}/${folderName}.txt --format unity --sheet ${DEST_FOLDER}/${folderName}.png --disable-rotation --no-trim --reduce-border-artifacts

#         fi
#     done
fi
if [ "$3" = "alpha" ]; then
    cp ${DEST_FOLDER}/$2/$2.png ${DEST_FOLDER}/$2/$2_alpha.png
    echo "copy ${DEST_FOLDER}/${folderName}.png to ${DEST_FOLDER}/${folderName}_alpha.png"
fi
