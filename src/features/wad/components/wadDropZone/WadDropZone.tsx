import React from 'react';
import { PiArchiveTrayDuotone } from 'react-icons/pi';

import { useMountWads } from '../..';
import { useFileDrop } from '../../../../hooks';

export const WadDropZone: React.FC = () => {
  const { mutate: mountWadsMutate } = useMountWads();

  const isDroppingFile = useFileDrop({
    onFileDrop: (paths) => {
      mountWadsMutate({ wadPaths: paths });
    },
  });

  if (!isDroppingFile) {
    return null;
  }

  return (
    <div className="fixed left-0 top-0 z-50 flex h-[100vh] w-[100vw] animate-fadeIn items-center justify-center bg-gray-800/50 transition-opacity">
      <PiArchiveTrayDuotone className="h-64 w-64 fill-gray-200/50" />
    </div>
  );
};
