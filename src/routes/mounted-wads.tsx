import { generatePath, useNavigate } from 'react-router-dom';

import { Spinner } from '../components';
import { WadDropZone, WadTabs, useMountedWads } from '../features/wad';
import { appRoutes } from '../lib/router';

export default function MountedWadItem() {
  const naviagte = useNavigate();

  const mountedWadsQuery = useMountedWads();

  const handleSelectedWadChanged = (selectedWad: string) => {
    naviagte(generatePath(appRoutes.mountedWad, { wadId: selectedWad }));
  };

  return (
    <div className="flex w-full px-2 py-2">
      {mountedWadsQuery.isLoading && <Spinner />}
      {mountedWadsQuery.isSuccess && (
        <>
          <WadDropZone />
          <WadTabs
            wads={mountedWadsQuery.data.wads}
            onSelectedWadChanged={handleSelectedWadChanged}
          />
        </>
      )}
    </div>
  );
}
