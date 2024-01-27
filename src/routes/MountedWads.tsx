import { useSearchParams } from 'react-router-dom';

import { WadDropZone, WadTabs } from '../features/wad';

export default function MountedWadItem() {
  const [searchParams, setSearchParams] = useSearchParams();

  const handleSelectedWadChanged = (selectedWad: string) => {
    setSearchParams({ wadId: selectedWad });
  };

  return (
    <div className="flex w-full px-2 py-2">
      <WadDropZone />
      <WadTabs
        selectedWad={searchParams.get('wadId') ?? undefined}
        onSelectedWadChanged={handleSelectedWadChanged}
        selectedItemId={searchParams.get('itemId') ?? undefined}
      />
    </div>
  );
}
