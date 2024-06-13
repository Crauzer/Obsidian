import { generatePath, useNavigate } from 'react-router-dom';

import { WadDropZone, WadTabs } from '../features/wad';
import { appRoutes } from '../lib/router';

export default function MountedWadItem() {
  const naviagte = useNavigate();

  const handleSelectedWadChanged = (selectedWad: string) => {
    naviagte(generatePath(appRoutes.mountedWad, { wadId: selectedWad }));
  };

  return (
    <div className="flex w-full px-2 py-2">
      <WadDropZone />
      <WadTabs selectedWad={undefined} onSelectedWadChanged={handleSelectedWadChanged} />
    </div>
  );
}
