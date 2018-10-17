# set params
SRC_FOLDER=${PWD}/Config
DEST_FOLDER=${PWD}/Assets/Resources/Config

for file in $SRC_FOLDER/*
do
    if [ -f $file ]; then
    	
    	fileName=${file##*/}
    	
    	if [ ${fileName##*.} == "json" ]; then
    		echo "Sync File: $fileName"
    		mv ${SRC_FOLDER}/${fileName} ${DEST_FOLDER}/${fileName%.*}.txt
    	fi

    fi
done 