import { useParams } from 'react-router-dom';

import { WadTree } from '../features/wad';

export default function Wad() {
  const { wadId } = useParams();

  if (!wadId) {
    return null;
  }

  return <WadTree wadId={wadId} />;
}
