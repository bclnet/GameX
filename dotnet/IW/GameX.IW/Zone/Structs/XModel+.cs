﻿using static GameX.IW.Zone.Asset;

namespace GameX.IW.Zone {
    public unsafe partial struct XModel {
        public static void writeXModel(ZoneInfo info, ZStream buf, XModel* data) {
            var materialOffs = stackalloc int[data->numSurfaces];
            for (var i = 0; i < data->numSurfaces; i++)
                materialOffs[i] = ZoneWriter.requireAsset(info, UnkAssetType.MATERIAL, new string(data->materials[i]->name), buf);

            fixed (byte* _ = buf.at) {
                var dest = (XModel*)_;
                buf.write((byte*)data, sizeof(XModel), 1);
                buf.pushStream(ZSTREAM.VIRTUAL);

                buf.write(data->name, strlen(data->name) + 1, 1);
                dest->name = (char*)-1;

                if (data->boneNames != null) {
                    buf.align(ZStream.ALIGN_TO_2);
                    buf.write((byte*)data->boneNames, sizeof(short), dest->numBones);
                    dest->boneNames = (short*)-1;
                }

                if (data->parentList != null) {
                    buf.align(ZStream.ALIGN_TO_1);
                    buf.write(dest->parentList, 1, dest->numBones - dest->numRootBones);
                    dest->parentList = (char*)-1;
                }

                if (data->tagAngles != null) {
                    buf.align(ZStream.ALIGN_TO_2);
                    buf.write((byte*)dest->tagAngles, sizeof(XModelAngle), dest->numBones - dest->numRootBones);
                    dest->tagAngles = (XModelAngle*)-1;
                }

                if (data->tagPositions != null) {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write((byte*)dest->tagPositions, sizeof(XModelTagPos), dest->numBones - dest->numRootBones);
                    dest->tagPositions = (XModelTagPos*)-1;
                }

                if (data->partClassification != null) {
                    buf.align(ZStream.ALIGN_TO_1);
                    buf.write(dest->partClassification, 1, dest->numBones);
                    dest->partClassification = (char*)-1;
                }

                if (data->animMatrix != null) {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write((byte*)dest->animMatrix, sizeof(DObjAnimMat), dest->numBones);
                    dest->animMatrix = (DObjAnimMat*)-1;
                }

                if (data->materials != null) {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write((byte*)materialOffs, sizeof(int), dest->numSurfaces); // should be just the offsets
                    dest->materials = (Material**)-1;
                }

                for (var i = 0; i < 4; i++) {
                    if (data->lods(i)->surfaces == null) continue;

                    fixed (byte* _2 = buf.at) {
                        var surfs = (XModelSurfaces*)_2;
                        buf.write((byte*)dest->lods(i)->surfaces, sizeof(XModelSurfaces), 1);
                        buf.write((byte*)dest->lods(i)->surfaces->name, strlen(dest->lods(i)->surfaces->name) + 1, 1);

                        if (surfs->surfaces != null)
                            fixed (byte* _3 = buf.at) {
                                buf.align(ZStream.ALIGN_TO_4);
                                var surf = (XSurface*)_3;
                                buf.write((byte*)dest->lods(i)->surfaces->surfaces, sizeof(XSurface) * surfs->numSurfaces, 1);

                                for (var j = 0; j < surfs->numSurfaces; j++) {
                                    if (surf[j].blendInfo != null) // OffsetToPointer
                                    {
                                        buf.align(ZStream.ALIGN_TO_2);
                                        buf.write(surf[j].blendInfo, sizeof(short), surf[j].blendNum1 +
                                            (surf[j].blendNum2 * 3) + (surf[j].blendNum3 * 5) + (surf[j].blendNum4 * 7));
                                        surf[j].blendInfo = (char*)-1;
                                    }

                                    if (surf[j].vertexBuffer != null) // OffsetToPointer
                                    {
                                        buf.pushStream(ZSTREAM.VERTEX);
                                        buf.align(ZStream.ALIGN_TO_16);
                                        buf.write((byte*)surf[j].vertexBuffer, 32, surf[j].numVertices);
                                        surf[j].vertexBuffer = (GfxPackedVertex*)-1;
                                        buf.popStream();
                                    }

                                    if (surf[j].ct != null)
                                        fixed (byte* _4 = buf.at) {
                                            buf.align(ZStream.ALIGN_TO_4);
                                            var ct = (XSurfaceCT*)_4;
                                            buf.write((byte*)surf[j].ct, 12, surf[j].numCT);

                                            for (var k = 0; k < surf[j].numCT; k++)
                                                if (ct[k].entry != null) // OffsetToPointer
                                                    fixed (byte* _5 = buf.at) {
                                                        buf.align(ZStream.ALIGN_TO_4);
                                                        var entry = (XSurfaceCTEntry*)_5;
                                                        buf.write((byte*)ct[k].entry, 40, 1);

                                                        if (entry->node != null) {
                                                            buf.write(entry->node, 16, entry->numNode);
                                                            entry->node = (char*)-1;
                                                        }

                                                        if (entry->leaf != null) {
                                                            buf.write((byte*)entry->leaf, 2, entry->numLeaf);
                                                            entry->leaf = (short*)-1;
                                                        }

                                                        ct[k].entry = (XSurfaceCTEntry*)-1;
                                                    }

                                            surf[j].ct = (XSurfaceCT*)-1;
                                        }

                                    if (surf[j].indexBuffer != null) {
                                        buf.pushStream(ZSTREAM.INDEX);
                                        buf.align(ZStream.ALIGN_TO_16);
                                        buf.write((byte*)surf[j].indexBuffer, 6, surf[j].numPrimitives);
                                        surf[j].indexBuffer = (Face*)-1;
                                        buf.popStream();
                                    }
                                }


                                surfs->name = (char*)-1;
                                dest->lods(i)->surfaces = (XModelSurfaces*)-1;
                                surfs->surfaces = (XSurface*)-1;
                            }
                    }
                }

                if (data->colSurf != null) {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write((byte*)dest->colSurf, sizeof(XColSurf), dest->numColSurfs);

                    for (var i = 0; i < dest->numColSurfs; i++) {
                        buf.align(ZStream.ALIGN_TO_4);
                        buf.write((byte*)dest->colSurf[i].tris, 48, dest->colSurf[i].count);
                        dest->colSurf[i].tris = (void*)-1;
                    }

                    dest->colSurf = (XColSurf*)-1;
                }

                if (data->boneInfo != null) {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write(dest->boneInfo, 28, dest->numBones);
                    dest->boneInfo = (char*)-1;
                }

                if (dest->physPreset != null) {
                    buf.align(ZStream.ALIGN_TO_4);
                    PhysPreset.writePhysPreset(info, buf, dest->physPreset);
                    dest->physPreset = (PhysPreset*)-1;
                }

                if (dest->physCollmap != null) {
                    buf.align(ZStream.ALIGN_TO_4);
                    PhysGeomList.writePhysCollmap(info, buf, dest->physCollmap);
                    dest->physCollmap = (PhysGeomList*)-1;
                }

                buf.popStream();
            }
        }

        //void* addXModel(zoneInfo_t* info, const char* name, char* data, int dataLen)
        //{
        //	if (data == NULL) return NULL;

        //	if(dataLen < 0)
        //	{
        //		XModel * model = (XModel*)data;
        //		short* boneNames = new short[model->numBones];

        //		for(int i=0; i<model->numBones; i++)
        //		{
        //			boneNames[i] = addScriptString(info, SL_ConvertToString(model->boneNames[i]));
        //		}

        //		model->boneNames = boneNames;

        //		for(int i=0; i<model->numSurfaces; i++)
        //		{
        //			// allow material overriding
        //			void* file;
        //			void* asset;

        //			if(int len = FS_ReadFile(va("zonebuilder/materials/%s.txt", model->materials[i]->name), &file) > 0)
        //			{
        //				asset = addMaterial(info, model->materials[i]->name, (char*)file, len);
        //				FS_FreeFile(file);
        //			}
        //			else
        //			{
        //				asset = addMaterial(info, model->materials[i]->name, (char*)model->materials[i], -1);
        //			}

        //			addAsset(info, ASSET_TYPE_MATERIAL, model->materials[i]->name, asset);
        //		}

        //		return data;
        //	}

        //	// copy stuff over
        //	XModel * base = (XModel*)DB_FindXAssetHeader(ASSET_TYPE_XMODEL, "viewmodel_mp5k");
        //	XModel * asset = new XModel;

        //	memcpy(asset, base, sizeof(XModel));
        //	asset->lods[0].surfaces = new XModelSurfaces;
        //	memcpy(asset->lods[0].surfaces, base->lods[0].surfaces, sizeof(XModelSurfaces));

        //	XModelSurfaces * surf = asset->lods[0].surfaces;
        //	surf->name = new char[strlen(name) + 6];
        //	sprintf((char*)surf->name, "%s_surf", name);

        //	BUFFER * buf = new BUFFER(data, dataLen);
        //	asset->name = new char[128];
        //	buf.readstr(asset->name, 128);
        //	buf.read(&asset->numBones, 4, 1);
        //	buf.read(&asset->numRootBones, 4, 1);
        //	buf.read(&asset->numSurfaces, 4, 1);
        //	surf->numSurfaces = asset->numSurfaces;
        //	asset->lods[0].numSurfs = surf->numSurfaces;

        //	asset->boneNames = new short[asset->numBones];

        //	for(int i=0; i<asset->numBones; i++)
        //	{
        //		char bone[64];
        //		buf.readstr(bone, 64);
        //		asset->boneNames[i] = addScriptString(info, bone);
        //	}

        //	// allocate stuff and load it
        //	if (asset->numBones - asset->numRootBones)
        //	{
        //		asset->parentList = new char[asset->numBones - asset->numRootBones];
        //		asset->tagAngles = new XModelAngle[asset->numBones - asset->numRootBones];
        //		asset->tagPositions = new XModelTagPos[asset->numBones - asset->numRootBones];

        //		buf.read(asset->parentList, sizeof(char), asset->numBones - asset->numRootBones);
        //		buf.read(asset->tagAngles, sizeof(XModelAngle), asset->numBones - asset->numRootBones);
        //		buf.read(asset->tagPositions, sizeof(XModelTagPos), asset->numBones - asset->numRootBones);
        //	}

        //	if(asset->numBones)
        //	{
        //		asset->partClassification = new char[asset->numBones];
        //		asset->animMatrix = new DObjAnimMat[asset->numBones];

        //		buf.read(asset->partClassification, sizeof(char), asset->numBones);
        //		buf.read(asset->animMatrix, sizeof(DObjAnimMat), asset->numBones);
        //	}

        //	surf->surfaces = new XSurface[surf->numSurfaces];
        //	memset(surf->surfaces, 0, sizeof(XSurface) * surf->numSurfaces);

        //	for(int i=0; i<surf->numSurfaces; i++)
        //	{
        //		XSurface* s = &surf->surfaces[i];
        //		buf.read(&s->numVertices, 4, 1);
        //		buf.read(&s->numPrimitives, 4, 1);
        //		buf.read(&s->blendNum1, 4, 1);
        //		buf.read(&s->blendNum2, 4, 1);
        //		buf.read(&s->blendNum3, 4, 1);
        //		buf.read(&s->blendNum4, 4, 1);

        //		int blendCount = (s->blendNum4 * 7) + (s->blendNum3 * 5) + (s->blendNum2 * 3) + s->blendNum1;

        //		if(blendCount)
        //		{
        //			s->blendInfo = new char[blendCount * 2];
        //			buf.read(s->blendInfo, 2, blendCount);
        //		}
        //		else 
        //		{
        //			s->blendInfo = NULL;	
        //		}

        //		s->vertexBuffer = new GfxPackedVertex[s->numVertices];
        //		buf.read(s->vertexBuffer, 32, s->numVertices);

        //		int ct = 0;
        //		buf.read(&ct, 4, 1);

        //		if(ct)
        //		{
        //			buf.read(&s->numCT, 4, 1);
        //			s->ct = new XSurfaceCT[s->numCT];

        //			for(int j=0; j<s->numCT; j++)
        //			{
        //				XSurfaceCT* ct = &s->ct[j];
        //				buf.read(&ct->pad, 4, 1);
        //				buf.read(&ct->pad2, 4, 1);
        //				ct->entry = new XSurfaceCTEntry;
        //				buf.read(ct->entry, 24, 1);
        //				buf.read(&ct->entry->numNode, 4, 1);
        //				buf.read(&ct->entry->numLeaf, 4, 1);

        //				if(ct->entry->numNode)
        //				{
        //					ct->entry->node = new char[ct->entry->numNode * 16];
        //					buf.read(ct->entry->node, 16, ct->entry->numNode);
        //				}
        //				else
        //				{
        //					ct->entry->node = NULL;
        //				}

        //				if(ct->entry->numLeaf)
        //				{
        //					ct->entry->leaf = new short[ct->entry->numLeaf];
        //					buf.read(ct->entry->leaf, 2, ct->entry->numLeaf);
        //				}
        //				else
        //				{
        //					ct->entry->node = NULL;
        //				}
        //			}
        //		}
        //		else
        //		{
        //			s->ct = NULL;
        //			s->numCT = 0;
        //		}

        //		s->indexBuffer = new Face[s->numPrimitives];
        //		buf.read(s->indexBuffer, sizeof(Face), s->numPrimitives);
        //	}

        //	asset->materials = new Material*[asset->numSurfaces];

        //	// read the material stuff and load a material if we need it
        //	for(int i=0; i<asset->numSurfaces; i++)
        //	{
        //		char matName[64] = { 0 };
        //		char techName[64] = { 0 };
        //		char matFileName[78] = { 0 };

        //		buf.readstr(matName, 50);
        //		buf.readstr(techName, 64);

        //		char* filename = matName;

        //		// asset is already in db... dont re-add it
        //		if (containsAsset(info, ASSET_TYPE_MATERIAL, matName) > 0)
        //		{
        //			asset->materials[i] = (Material*)getAsset(info, ASSET_TYPE_MATERIAL, matName);
        //			continue;
        //		}

        //		if(!strncmp("mc/", matName, 3)) filename = matName + 3;

        //		_snprintf(matFileName, sizeof(matFileName), "materials/%s.txt", filename);

        //		void* matBuf;
        //		int len = FS_ReadFile(matFileName, &matBuf);

        //		if(len > 0)
        //		{
        //			asset->materials[i] = (Material*)addMaterial(info, matName, (char*)matBuf, len);
        //			FS_FreeFile(matBuf);
        //		}
        //		else
        //		{
        //			asset->materials[i] = (Material*)DB_FindXAssetHeader(ASSET_TYPE_MATERIAL, matName);
        //			addMaterial(info, matName, (char*)asset->materials[i], -1);
        //		}

        //		addAsset(info, ASSET_TYPE_MATERIAL, matName, asset->materials[i]);		
        //	}

        //	int test = 0;
        //	buf.read(&test, 4, 1);

        //	if(test) Com_Error(false, "Cause NTA said so!");

        //	buf.read(&test, 4, 1);

        //	if(!test) Com_Error(false, "Cause NTA said so!");

        //	asset->boneInfo = new char[asset->numBones * 28];
        //	buf.read(asset->boneInfo, 28, asset->numBones);

        //	return asset;
        //}
    }
}