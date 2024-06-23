import clsx from 'clsx';
import { BsFillFileEarmarkImageFill, BsTable } from 'react-icons/bs';
import { FaFile } from 'react-icons/fa';
import {
  PiBoneDuotone,
  PiCodeDuotone,
  PiCubeFocusFill,
  PiFileAudioDuotone,
  PiFilePlusDuotone,
  PiGridFourDuotone,
} from 'react-icons/pi';
import { PiFileSvgFill } from 'react-icons/pi';
import { match } from 'ts-pattern';

import { AnimationIcon, ForestIcon } from '../../assets';
import { LeagueFileKind } from './types';

export const getLeagueFileKindIcon = (kind: LeagueFileKind) => {
  return match(kind)
    .with('animation', () => AnimationIcon)
    .with('jpeg', () => BsFillFileEarmarkImageFill)
    .with('lua_obj', () => PiCodeDuotone)
    .with('map_geometry', () => ForestIcon)
    .with('png', () => BsFillFileEarmarkImageFill)
    .with('preload', () => PiCodeDuotone)
    .with('property_bin', () => PiCodeDuotone)
    .with('riot_string_table', () => BsTable)
    .with('simple_skin', () => PiCubeFocusFill)
    .with('skeleton', () => PiBoneDuotone)
    .with('static_mesh_ascii', () => PiCubeFocusFill)
    .with('static_mesh_binary', () => PiCubeFocusFill)
    .with('texture', () => BsFillFileEarmarkImageFill)
    .with('texture_dds', () => BsFillFileEarmarkImageFill)
    .with('unknown', () => FaFile)
    .with('world_geometry', () => ForestIcon)
    .with('wwise_bank', () => PiFileAudioDuotone)
    .with('wwise_package', () => PiFileAudioDuotone)
    .with('light_grid', () => PiGridFourDuotone)
    .with('property_bin_override', () => PiFilePlusDuotone)
    .with('svg', () => PiFileSvgFill)
    .exhaustive();
};

export const getLeagueFileKindIconColor = (kind: LeagueFileKind) => {
  return match(kind)
    .with('animation', () => clsx('fill-red-500'))
    .with('jpeg', () => clsx('fill-green-600'))
    .with('lua_obj', () => clsx('fill-teal-500'))
    .with('map_geometry', () => clsx('fill-green-600'))
    .with('png', () => clsx('fill-green-600'))
    .with('preload', () => clsx('fill-red-500'))
    .with('property_bin', () => clsx('fill-teal-500'))
    .with('riot_string_table', () => clsx('fill-yellow-500'))
    .with('simple_skin', () => clsx('fill-purple-500'))
    .with('skeleton', () => clsx('fill-gray-100'))
    .with('static_mesh_ascii', () => clsx('fill-purple-500'))
    .with('static_mesh_binary', () => clsx('fill-purple-500'))
    .with('texture', () => clsx('fill-green-600'))
    .with('texture_dds', () => clsx('fill-green-600'))
    .with('unknown', () => clsx('fill-red-500'))
    .with('world_geometry', () => clsx('fill-green-600'))
    .with('wwise_bank', () => clsx('fill-red-500'))
    .with('wwise_package', () => clsx('fill-red-500'))
    .with('light_grid', () => clsx('fill-yellow-500'))
    .with('property_bin_override', () => clsx('fill-blue-500'))
    .with('svg', () => clsx('fill-green-600'))
    .exhaustive();
};
