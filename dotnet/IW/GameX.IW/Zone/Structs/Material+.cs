﻿using static GameX.IW.Zone.Asset;

namespace GameX.IW.Zone {
    public unsafe partial struct Material {
        public static void writeGfxImage(ZoneInfo info, ZStream buf, GfxImage* data) {
            fixed (byte* _ = buf.at) {
                var img = (GfxImage*)_;
                buf.write((byte*)data, sizeof(GfxImage), 1);
                buf.pushStream(ZSTREAM.VIRTUAL);

                buf.write(data->name, strlen(data->name) + 1, 1);
                img->name = (char*)-1;

                if (data->texture != null) // OffsetToPointer
                {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write((byte*)data->texture, sizeof(GfxImageLoadDef), 1);
                    img->texture = (GfxImageLoadDef*)-1;
                }

                buf.popStream(); // VIRTUAL
            }
        }

        public static void writeMaterial(ZoneInfo info, ZStream buf, Material* data) {
            fixed (byte* _ = buf.at) {
                // require this asset
                var techsetOffset = ZoneWriter.requireAsset(info, UnkAssetType.TECHSET, new string(data->techniqueSet->name), buf);

                var dest = (Material*)_;
                buf.write((byte*)data, sizeof(Material), 1);
                buf.pushStream(ZSTREAM.VIRTUAL);

                buf.write(data->name, strlen(data->name) + 1, 1);
                dest->name = (char*)-1;

                // write techset here
                // we are just going to require it and use the offset
                dest->techniqueSet = (MaterialTechniqueSet*)(techsetOffset);

                // write texturedefs here
                if (data->textureTable != null) {
                    buf.align(ZStream.ALIGN_TO_4);
                    for (var i = 0; i < data->textureCount; i++)
                        fixed (byte* _2 = buf.at) {
                            var tex = (MaterialTextureDef*)_2;
                            buf.write((byte)&data->textureTable[i], sizeof(MaterialTextureDef), 1);
                            tex->info.image = (GfxImage*)-1;
                        }

                    for (var i = 0; i < data->textureCount; i++) {
                        // TODO, make with work with water images too
                        buf.pushStream(ZSTREAM.TEMP);
                        buf.align(ZStream.ALIGN_TO_4);
                        writeGfxImage(info, buf, data->textureTable[i].info.image);
                        buf.popStream();
                    }
                    dest->textureTable = (MaterialTextureDef*)-1;
                }

                if (data->constantTable != null) // OffsetToPointer
                {
                    buf.align(ZStream.ALIGN_TO_16);
                    buf.write((char*)data->constantTable, data->constantCount * sizeof(MaterialConstantDef), 1);
                    dest->constantTable = (MaterialConstantDef*)-1;
                }

                if (data->stateBitTable != null) // OffsetToPointer
                {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write((byte*)data->stateBitTable, data->stateBitsCount * 8, 1);
                    dest->stateBitTable = (void*)-1;
                }
            }
        }

        //char baseMatName[64];
        //Dictionary<string, string> materialMaps;
        //Dictionary<string, vec4*> materialProperties;

        //    // finally went and made this one sane with our new class for csv files
        //    int parseMatFile(char* data, size_t dataLen)
        //    {
        //        CSVFile* file = new CSVFile(data, dataLen);
        //        int curRow = 0;
        //        char* param = file->getData(curRow, 0);

        //        // new material format
        //        if (!strcmp(param, "map") || !strcmp(param, "prop"))
        //        {
        //            while (param != NULL)
        //            {
        //                if (!strcmp(param, "map"))
        //                {
        //                    param = file->getData(curRow, 1);
        //                    if (!strcmp("basemat", param))
        //                    {
        //                        strncpy(baseMatName, file->getData(curRow, 2), sizeof(baseMatName));
        //                        curRow++;
        //                        param = file->getData(curRow, 0);
        //                        continue;
        //                    }

        //                    materialMaps[param] = file->getData(curRow, 2);
        //                }

        //                if (!strcmp(param, "prop"))
        //                {
        //                    char vecstr[256];
        //                    strncpy(vecstr, file->getData(curRow, 2), 256);
        //                    vec4_t* vec = (vec4_t*)malloc(sizeof(vec4_t)); // this is a memory leak but idk where to free it
        //                    sscanf_s(vecstr, "(%g, %g, %g, %g)", vec[0], vec[1], vec[2], vec[3]);
        //                    materialProperties[file->getData(curRow, 1)] = vec;
        //                }

        //                curRow++;
        //                param = file->getData(curRow, 0);
        //            }
        //        }

        //        // fallback format
        //        while (param != NULL)
        //        {
        //            if (!strcmp("basemat", param))
        //            {
        //                strncpy(baseMatName, file->getData(curRow, 1), sizeof(baseMatName));
        //                curRow++;
        //                param = file->getData(curRow, 0);
        //                continue;
        //            }

        //            materialMaps[param] = file->getData(curRow, 1);

        //            curRow++;
        //            param = file->getData(curRow, 0);
        //        }
        //        return 0;
        //    }
        //}

        //_IWI* LoadIWIHeader(const char* name)
        //{

        //    char fname[64] = { 0 };
        //        _snprintf(fname, sizeof(fname), "images/%s.iwi", name);

        //        _IWI* buf = new _IWI;
        //        int handle = 0;

        //        FS_FOpenFileRead(fname, &handle, 1);

        //	if (handle == 0)
        //	{
        //		Com_Error(1, "Image does not exist: %s!", fname);
        //        delete buf;
        //		return NULL;
        //	}

        //    FS_Read(buf, sizeof(_IWI), handle);
        //    FS_FCloseFile(handle);
        //	return buf;
        //}

        //GfxImageLoadDef* GenerateLoadDef(GfxImage* image, short iwi_format)
        //{
        //    GfxImageLoadDef* texture = new GfxImageLoadDef;
        //    memset(texture, 0, sizeof(GfxImageLoadDef));

        //    switch (iwi_format)
        //    {
        //        case IWI_ARGB:
        //            texture->format = 21;
        //            break;
        //        case IWI_RGB8:
        //            texture->format = 20;
        //            break;
        //        case IWI_DXT1:
        //            texture->format = 0x31545844;
        //            break;
        //        case IWI_DXT3:
        //            texture->format = 0x33545844;
        //            break;
        //        case IWI_DXT5:
        //            texture->format = 0x35545844;
        //            break;
        //    }
        //    texture->dimensions[0] = image->width;
        //    texture->dimensions[1] = image->height;
        //    texture->dimensions[2] = image->depth;

        //    return texture;
        //}

        //GfxImage* LoadImageFromIWI(const char* name, char semantic, char category, char flags)
        //{
        //    GfxImage* ret = new GfxImage;
        //    memset(ret, 0, sizeof(GfxImage));

        //    _IWI* buf = LoadIWIHeader(name);

        //    ret->height = buf.xsize;
        //    ret->width = buf.ysize;
        //    ret->depth = buf.depth;
        //    ret->dataLen1 = buf.mipAddr4 - 32;
        //    ret->dataLen2 = buf.mipAddr4 - 32;
        //    ret->name = strdup(name);
        //    ret->semantic = semantic;
        //    ret->category = category;
        //    ret->flags = flags;
        //    ret->mapType = 3; // hope that works lol

        //    ret->texture = GenerateLoadDef(ret, buf.format);
        //    delete buf;
        //    return ret;
        //}

        //void* addMaterial(zoneInfo_t* info, const char* name, char* data, int dataLen)
        //{
        //    if (data == NULL) return NULL;

        //    if (dataLen < 0)
        //    {
        //        Material* mat = (Material*)data;
        //        for (int i = 0; i < mat->textureCount; i++)
        //        {
        //            _IWI* buf = LoadIWIHeader(mat->textureTable[i].info.image->name);
        //            mat->textureTable[i].info.image->texture = GenerateLoadDef(mat->textureTable[i].info.image, buf.format);
        //            delete buf;
        //        }
        //        addAsset(info, ASSET_TYPE_TECHSET, mat->techniqueSet->name, addTechset(info, mat->techniqueSet->name, (char*)mat->techniqueSet, -1));
        //        return data;
        //    }
        //    else
        //    {
        //        parseMatFile(data, dataLen);
        //    }

        //    Material* basemat = (Material*)DB_FindXAssetHeader(ASSET_TYPE_MATERIAL, baseMatName);

        //    // duplicate the material
        //    Material* mat = new Material;
        //    memcpy(mat, basemat, sizeof(Material));

        //    // new info
        //    mat->name = strdup(name);

        //    mat->textureTable = new MaterialTextureDef[materialMaps.size()];
        //    mat->textureCount = materialMaps.size();

        //    int i = 0;
        //    for (auto it = materialMaps.begin(); it != materialMaps.end(); ++it)
        //    {
        //        MaterialTextureDef* cur = &mat->textureTable[i];
        //        memset(cur, 0, sizeof(MaterialTextureDef));
        //        switch (R_HashString(it->first.c_str()))
        //        {
        //            case HASH_COLORMAP: cur->nameStart = 'c'; cur->nameHash = HASH_COLORMAP; cur->semantic = SEMANTIC_COLOR_MAP; break;
        //            case HASH_DETAILMAP: cur->nameStart = 'd'; cur->nameHash = HASH_DETAILMAP; cur->semantic = SEMANTIC_FUNCTION; break;
        //            case HASH_SPECULARMAP: cur->nameStart = 's'; cur->nameHash = HASH_SPECULARMAP; cur->semantic = SEMANTIC_SPECULAR_MAP; break;
        //            case HASH_NORMALMAP: cur->nameStart = 'n'; cur->nameHash = HASH_NORMALMAP; cur->semantic = SEMANTIC_NORMAL_MAP; break;
        //        }
        //        cur->nameEnd = 'p';
        //        cur->info.image = LoadImageFromIWI(it->second.c_str(), cur->semantic, 0, 0);
        //        i++;
        //    }

        //    // only overwrite this stuff if we specifically want to
        //    if (materialProperties.size())
        //    {
        //        mat->constantTable = new MaterialConstantDef[materialProperties.size()];
        //        mat->constantCount = materialProperties.size();

        //        i = 0;
        //        for (auto it = materialProperties.begin(); it != materialProperties.end(); ++it)
        //        {
        //            MaterialConstantDef* cur = &mat->constantTable[i];
        //            memset(cur, 0, sizeof(MaterialConstantDef));
        //            strncpy(cur->name, it->first.c_str(), 12);
        //            cur->nameHash = R_HashString(it->first.c_str());
        //            memcpy(cur->literal, it->second, sizeof(vec4_t));

        //            i++;
        //        }
        //    }

        //    // add techset to our DB here
        //    // this one is weird and is all handled internally cause of the shit it does
        //    addAsset(info, ASSET_TYPE_TECHSET, mat->techniqueSet->name, addTechset(info, mat->techniqueSet->name, (char*)mat->techniqueSet, -1));
        //    return mat;
        //}

        //void* addGfxImage(zoneInfo_t* info, const char* name, char* data, int dataLen)
        //{
        //    if (dataLen > 0)
        //    {
        //        GfxImage* ret = LoadImageFromIWI(name, SEMANTIC_COLOR_MAP, 0, 0);
        //        return ret;
        //    }

        //    // need to fix it to have a correct loadDef here
        //    char fname[64] = { 0 };
        //    _snprintf(fname, sizeof(fname), "images/%s.iwi", name);

        //    _IWI* buf = LoadIWIHeader(name);

        //    GfxImage* ret = new GfxImage;
        //    memcpy(ret, data, sizeof(GfxImage));

        //    ret->texture = GenerateLoadDef(ret, buf.format);

        //    delete buf;

        //    return ret;
        //}
    }
}