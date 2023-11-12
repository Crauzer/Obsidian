import { useSearchParams } from 'react-router-dom';

import { WadTabs } from '../features/wad';

export default function MountedWadItem() {
  const [searchParams, setSearchParams] = useSearchParams();

  const handleSelectedWadChanged = (selectedWad: string) => {
    setSearchParams({ wadId: selectedWad });
  };

  return (
    <WadTabs
      selectedWad={searchParams.get('wadId') ?? undefined}
      onSelectedWadChanged={handleSelectedWadChanged}
      selectedItemId={searchParams.get('itemId') ?? undefined}
    />
  );
}
