import { forwardRef } from 'react';
import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { FaSave } from 'react-icons/fa';
import { VscSave } from 'react-icons/vsc';
import { v4 as uuidv4 } from 'uuid';

import { useExtractWadItemsWithDirectory } from '../../..';
import { ContextMenu, Icon, LoadingOverlay } from '../../../../../components';
import { useActionProgress } from '../../../../actions';
import { WadItem } from '../../../types';

type ExtractItemProps = {
  wadId: string;
  parentItemId?: string;
  item: WadItem;
};

export const ExtractItem = forwardRef<HTMLDivElement, ExtractItemProps>(
  ({ wadId, parentItemId, item }, ref) => {
    const [t] = useTranslation('wad');

    const [actionId] = useState<string>(uuidv4());

    const { isExtracting, extractWadItemsWithDirectory } = useExtractWadItemsWithDirectory();
    const actionProgress = useActionProgress(actionId);

    const progress = useMemo(() => {
      if (actionProgress.isSuccess) {
        return actionProgress.data?.payload.progress;
      }

      return 0;
    }, [actionProgress.data?.payload.progress, actionProgress.isSuccess]);

    return (
      <>
        <ContextMenu.Item
          ref={ref}
          className="flex flex-row items-center gap-2"
          onClick={() => {
            extractWadItemsWithDirectory({ wadId, parentItemId, items: [item.id], actionId });
          }}
          onSelect={(e) => e.preventDefault()}
        >
          <Icon icon={VscSave} size="md" />
          {t('contextMenu.extract')}
        </ContextMenu.Item>
        <LoadingOverlay
          open={isExtracting}
          onOpenChange={() => {}}
          progress={progress * 100}
          message={actionProgress.data?.payload.message}
        />
      </>
    );
  },
);
